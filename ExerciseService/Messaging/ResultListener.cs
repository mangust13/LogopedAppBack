using ExerciseService.Services;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events.Exercises;
using System.Text;
using System.Text.Json;

namespace ExerciseService.Messaging;

public class ResultListener : IHostedService
{
    private readonly RabbitOptions _options;
    private readonly ProgressReporter _progressReporter;

    private IConnection? _connection;
    private IChannel? _channel;

    public ResultListener(
        IOptions<RabbitOptions> options,
        ProgressReporter progressReporter)
    {
        _options = options.Value;
        _progressReporter = progressReporter;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: ExchangeType.Topic);

        const string queueName = "exercise.analysis.completed";

        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: _options.Exchange,
            routingKey: "exercise.analysis.completed");

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                var evt = JsonSerializer.Deserialize<ExerciseAnalysisCompletedEvent>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (evt == null)
                    throw new InvalidOperationException("Empty event payload");

                await _progressReporter.ReportAsync(evt);

                await _channel.BasicAckAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false);
            }
            catch
            {
                await _channel.BasicNackAsync(
                    deliveryTag: ea.DeliveryTag,
                    multiple: false,
                    requeue: true);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: consumer);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Dispose();
        _connection?.Dispose();
        return Task.CompletedTask;
    }
}
