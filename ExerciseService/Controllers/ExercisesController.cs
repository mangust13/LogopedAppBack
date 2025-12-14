using ExerciseService.Contracts;
using ExerciseService.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ExerciseService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExercisesController : ControllerBase
{
    private readonly RabbitMqPublisher _publisher;
    private static readonly ActivitySource Activity = new("ExerciseService.Start");

    public ExercisesController(RabbitMqPublisher publisher)
    {
        _publisher = publisher;
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

    [HttpPost("upload-audio")]
    public async Task<IActionResult> UploadAudio([FromForm] UploadAudioRequest req)
    {
        var file = req.File;

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var root = Directory.GetCurrentDirectory();
        var uploadsFolder = Path.Combine(root, "Uploads");

        Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, file.FileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var message = new
        {
            ExerciseId = req.ExerciseId,
            UserId = req.UserId,
            AudioUrl = filePath,      // Абсолютний шлях
            ReferenceText = req.ReferenceText
        };

        await _publisher.PublishAsync(message);

        return Ok(new { filePath });
    }
}
