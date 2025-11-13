using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class ExerciseController : ControllerBase
{
    private readonly RabbitOptions _options;

    public ExerciseController(IOptions<RabbitOptions> options)
    {
        _options = options.Value;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartExercise([FromBody] ExerciseRequest request)
    {
        try
        {
            // Підключення до RabbitMQ
            var factory = new ConnectionFactory()
            {
                HostName = _options.Host,
                UserName = _options.UserName,
                Password = _options.Password
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            // Exchange
            await channel.ExchangeDeclareAsync(
                exchange: _options.Exchange,
                type: ExchangeType.Topic);

            // Формуємо повідомлення
            var message = new
            {
                ExerciseId = request.ExerciseId,
                UserId = request.UserId,
                AudioUrl = request.AudioUrl,
                ReferenceText = request.ReferenceText,
                Timestamp = DateTime.UtcNow,
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            await channel.BasicPublishAsync<BasicProperties>(
                exchange: _options.Exchange,
                routingKey: _options.AudioRoutingKey,
                mandatory: false,
                basicProperties: new BasicProperties(),
                body: body);

            return Ok(new { message = $"Вправа #{request.ExerciseId} відправлена на аналіз" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}