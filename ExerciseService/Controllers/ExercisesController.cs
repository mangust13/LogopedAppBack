using ExerciseService.Contracts;
using ExerciseService.Messaging;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ExerciseService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExercisesController : ControllerBase
{
    private readonly RabbitMqPublisher _publisher;
    private readonly IHttpClientFactory _clientFactory;
    private static readonly ActivitySource Activity = new("ExerciseService.Start");

    public ExercisesController(RabbitMqPublisher publisher, IHttpClientFactory clientFactory)
    {
        _publisher = publisher;
        _clientFactory = clientFactory;
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] ExerciseRequest request)
    {
        using var activity = Activity.StartActivity("Exercise Start");

        activity?.SetTag("exercise.id", request.ExerciseId);
        activity?.SetTag("user.id", request.UserId);

        var message = new
        {
            request.ExerciseId,
            request.UserId,
            request.AudioUrl,
            request.ReferenceText,
            Timestamp = DateTime.UtcNow
        };

        await _publisher.PublishAsync(message);

        return Ok(new { message = $"Exercise #{request.ExerciseId} has been sent for analysis." });
    }

    [HttpPost("start-http")]
    public async Task<IActionResult> StartHttp([FromBody] ExerciseRequest request)
    {
        var client = _clientFactory.CreateClient("SpeechAI");
        Console.WriteLine("HTTP → SpeechAI BaseAddress = " + client.BaseAddress);

        var result = await client.PostAsJsonAsync(
            "/api/ai/analyze",
            new
            {
                request.ExerciseId,
                request.UserId,
                request.AudioUrl,
                request.ReferenceText
            });

        if (!result.IsSuccessStatusCode)
            return StatusCode(500, new { error = "SpeechAIService HTTP error" });

        var analysis = await result.Content.ReadFromJsonAsync<ExerciseResultDto>();

        return Ok(new
        {
            message = $"Exercise #{request.ExerciseId} analyzed over HTTP.",
            analysis
        });
    }
}
