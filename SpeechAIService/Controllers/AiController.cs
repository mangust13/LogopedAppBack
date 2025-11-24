using Microsoft.AspNetCore.Mvc;
using SpeechAIService.Contracts;

namespace SpeechAIService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly Random _random = new();

    [HttpPost("analyze")]
    public ActionResult<SpeechResultDto> Analyze([FromBody] SpeechRequest request)
    {
        var accuracy = _random.NextDouble() * 40 + 60;

        var ipa = request.ReferenceText;

        var result = new SpeechResultDto
        {
            ExerciseId = request.ExerciseId,
            UserId = request.UserId,
            AccuracyScore = Math.Round(accuracy, 1),
            RecognizedIPA = ipa,
            Feedback = $"Simulated HTTP analysis. Accuracy {accuracy:F1}%."
        };

        return Ok(result);
    }
}
