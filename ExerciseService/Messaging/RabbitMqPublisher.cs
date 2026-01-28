using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Contracts.Events.Common;
using System.Text;
using System.Text.Json;

namespace ExerciseService.Messaging;

public class RabbitMqPublisher
{
    private readonly RabbitOptions _options;

    public RabbitMqPublisher(IOptions<RabbitOptions> options)
    {
        _options = options.Value;
    }

    public async Task PublishAsync<TEvent>(
        TEvent @event,
        string routingKey)
        where TEvent : IntegrationEvent
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: ExchangeType.Topic);

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(@event));

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            CorrelationId = @event.CorrelationId,
            MessageId = @event.EventId.ToString()
        };

        await channel.BasicPublishAsync(
            exchange: _options.Exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body);
    }
}
