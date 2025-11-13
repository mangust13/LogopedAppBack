using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class ResultListener
{
    public static async Task StartAsync()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        const string exchangeName = "speech_exchange";
        const string queueName = "speech.result";

        await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic);
        await channel.QueueDeclareAsync(queueName, false, false, false, null);
        await channel.QueueBindAsync(queueName, exchangeName, "speech.result.*");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var result = JsonSerializer.Deserialize<JsonElement>(json);
            Console.WriteLine($"✅ Отримано результат для вправи {result.GetProperty("ExerciseId").GetString()}");
            Console.WriteLine($"Точність: {result.GetProperty("AccuracyScore").GetDouble():F1}%");
            Console.WriteLine($"Почуто (IPA): {result.GetProperty("RecognizedIPA").GetString()}");
        };

        await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
        Console.WriteLine("🎧 ExerciseService слухає результати...");
        Console.ReadLine();
    }
}
