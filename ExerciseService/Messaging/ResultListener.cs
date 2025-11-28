using ExerciseService.Services;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Diagnostics;

namespace ExerciseService.Messaging;

public class ResultListener : IHostedService
{
    private readonly RabbitOptions _options;
    private readonly ProgressReporter _progressReporter;

    private IConnection? _connection;
    private IChannel? _channel;

    public ResultListener(IOptions<RabbitOptions> options, ProgressReporter progressReporter)
    {
        _options = options.Value;
        _progressReporter = progressReporter;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            UserName = _options.UserName,
            Password = _options.Password
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: ExchangeType.Topic);

        const string queueName = "speech.result";

        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: _options.Exchange,
            routingKey: _options.ResultRoutingKey);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var payload = JsonSerializer.Deserialize<JsonElement>(json);

            var exerciseId = payload.GetProperty("ExerciseId").GetString();
            var userId = payload.GetProperty("UserId").GetString();
            var accuracy = payload.GetProperty("AccuracyScore").GetDouble();
            var feedback = payload.GetProperty("Feedback").GetString();
            var ipa = payload.GetProperty("RecognizedIPA").GetString();

            await _progressReporter.ReportAsync(
                userId!,
                exerciseId!,
                accuracy,
                feedback!,
                ipa!);
        };

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: true,
            consumer: consumer);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Dispose();
        _connection?.Dispose();
        return Task.CompletedTask;
    }
}
