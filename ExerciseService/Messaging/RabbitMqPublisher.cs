using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Diagnostics;

namespace ExerciseService.Messaging;

public class RabbitMqPublisher
{
    private readonly RabbitOptions _options;

    public RabbitMqPublisher(IOptions<RabbitOptions> options)
    {
        _options = options.Value;
    }

    public virtual async Task PublishAsync(object message)
    {
        Console.WriteLine($"Connecting to RabbitMQ at {_options.Host}:{_options.Port}");

        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(_options.Exchange, ExchangeType.Topic);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await channel.BasicPublishAsync<BasicProperties>(
            exchange: _options.Exchange,
            routingKey: _options.AudioRoutingKey,
            mandatory: false,
            basicProperties: new BasicProperties(),
            body: body
        );
    }
}
