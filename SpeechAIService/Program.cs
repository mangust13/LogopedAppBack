using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpeechAIService;

class SpeechAIService
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("speechai_log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            var azureKey = Environment.GetEnvironmentVariable("AZURE_SPEECH_KEY");
            var azureRegion = Environment.GetEnvironmentVariable("AZURE_SPEECH_REGION");
            var azureLanguage = "uk-UA";

            var factory = new ConnectionFactory() { HostName = "localhost" };
            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            const string exchangeName = "speech_exchange";
            await channel.ExchangeDeclareAsync(exchange: exchangeName, type: ExchangeType.Topic);
            await channel.QueueDeclareAsync("exercise.audio", false, false, false, null);
            await channel.QueueBindAsync("exercise.audio", exchangeName, "exercise.audio.*");

            Log.Information("[SpeechAIService] Очікування повідомлень...");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonSerializer.Deserialize<JsonElement>(json);

                    var exerciseId = message.GetProperty("ExerciseId").GetString();
                    var audioUrl = message.GetProperty("AudioUrl").GetString();
                    var referenceText = message.GetProperty("ReferenceText").GetString();

                    Log.Information($"🎧 Отримано вправу #{exerciseId}");
                    Log.Information($"🎙 Аудіо: {audioUrl}");
                    Log.Information($"📖 Еталонна фраза: \"{referenceText}\"");

                    // === Azure Speech Config ===
                    var config = SpeechConfig.FromSubscription(azureKey, azureRegion);
                    config.SpeechRecognitionLanguage = azureLanguage;
                    config.OutputFormat = OutputFormat.Detailed; // отримаємо сирий JSON

                    using var audioInput = AudioConfig.FromWavFileInput(audioUrl);
                    using var recognizer = new SpeechRecognizer(config, audioInput);

                    // === Pronunciation Assessment ===
                    var pronConfig = new PronunciationAssessmentConfig(
                        referenceText,
                        GradingSystem.HundredMark,
                        Granularity.Phoneme,
                        enableMiscue: true);
                    pronConfig.PhonemeAlphabet = "IPA";
                    pronConfig.EnableProsodyAssessment();
                    pronConfig.ApplyTo(recognizer);

                    // --- розпізнаємо ---
                    var result = await recognizer.RecognizeOnceAsync();

                    if (result.Reason == ResultReason.RecognizedSpeech)
                    {
                        // --- дістаємо raw lexical ---
                        string lexical = result.Text;
                        try
                        {
                            var rawJson = result.Properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult);
                            using var doc = JsonDocument.Parse(rawJson);
                            lexical = doc.RootElement
                                .GetProperty("NBest")[0]
                                .GetProperty("Lexical")
                                .GetString();
                        }
                        catch { }

                        var pron = PronunciationAssessmentResult.FromResult(result);

                        // --- текстова схожість ---
                        double textSimilarity = CalculateLevenshteinSimilarity(lexical, referenceText);

                        // --- збіг слів ---
                        var refWords = referenceText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        var recWords = lexical.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        var matched = recWords.Intersect(refWords, StringComparer.OrdinalIgnoreCase).Count();
                        double wordRecall = (double)matched / Math.Max(refWords.Length, 1);

                        // --- фінальні оцінки ---
                        double adjustedAccuracy = pron.AccuracyScore * Math.Min(textSimilarity, wordRecall);
                        double adjustedOverall = (pron.PronunciationScore * Math.Min(textSimilarity, wordRecall)
                                                  + pron.FluencyScore + pron.CompletenessScore) / 3;

                        Log.Information($"🧾 Raw (lexical): {lexical}");
                        Log.Information($"📃 Normalized: {result.Text}");
                        Log.Information($"🎯 Accuracy: {pron.AccuracyScore:F1} (скориг. {adjustedAccuracy:F1})");
                        Log.Information($"💬 Fluency: {pron.FluencyScore:F1}");
                        Log.Information($"🧩 Completeness: {pron.CompletenessScore:F1}");
                        Log.Information($"⭐ Overall (скориговано): {adjustedOverall:F1}");

                        var response = new
                        {
                            ExerciseId = exerciseId,
                            RecognizedText = lexical,
                            ReferenceText = referenceText,
                            Accuracy = adjustedAccuracy / 100.0,
                            Fluency = pron.FluencyScore / 100.0,
                            Completeness = pron.CompletenessScore / 100.0,
                            OverallScore = adjustedOverall / 100.0,
                            Feedback = $"Точність {adjustedAccuracy:F1}%, плавність {pron.FluencyScore:F1}%, повнота {pron.CompletenessScore:F1}%",
                            Timestamp = DateTime.UtcNow
                        };

                        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
                        await channel.BasicPublishAsync<BasicProperties>(
                            exchange: exchangeName,
                            routingKey: "speech.result.done",
                            mandatory: false,
                            basicProperties: new BasicProperties(),
                            body: body);

                        Log.Information($"📤 Результат для вправи #{exerciseId} відправлено.");
                    }
                    else
                    {
                        Log.Warning($"⚠️ Не вдалося розпізнати мову: {result.Reason}");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "❌ Помилка обробки повідомлення");
                }
            };

            await channel.BasicConsumeAsync("exercise.audio", autoAck: true, consumer);

            Console.WriteLine("Натисни Enter для виходу...");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "❌ Критична помилка запуску SpeechAIService");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    // === Левенштейн-порівняння ===
    static double CalculateLevenshteinSimilarity(string s1, string s2)
    {
        s1 = s1.ToLower().Trim();
        s2 = s2.ToLower().Trim();

        if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2)) return 1.0;
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return 0.0;

        int[,] d = new int[s1.Length + 1, s2.Length + 1];
        for (int i = 0; i <= s1.Length; i++) d[i, 0] = i;
        for (int j = 0; j <= s2.Length; j++) d[0, j] = j;

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        double distance = d[s1.Length, s2.Length];
        return 1.0 - (distance / Math.Max(s1.Length, s2.Length));
    }
}
