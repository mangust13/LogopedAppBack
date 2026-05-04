using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgressService.Contracts;
using ProgressService.Services;

namespace ProgressService.Controllers;

[ApiController]
[Route("game-progress")]
[Authorize]
public class GameProgressController : ControllerBase
{
    private readonly IGameProgressService _service;

    public GameProgressController(IGameProgressService service)
    {
        _service = service;
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete([FromBody] CompleteGameDto dto)
    {
        await _service.CompleteGameAsync(dto);
        return Ok();
    }

    [HttpGet("roadmap")]
    public async Task<IActionResult> GetRoadmap([FromQuery] int childId, [FromQuery] string sound)
    {
        if (string.IsNullOrWhiteSpace(sound))
            return BadRequest("sound is required");

        var result = await _service.GetRoadmapAsync(childId, sound);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary([FromQuery] int childId, [FromQuery] string sounds)
    {
        if (string.IsNullOrWhiteSpace(sounds))
            return BadRequest("sounds is required");

        var soundList = sounds.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var result = await _service.GetSoundsSummaryAsync(childId, soundList);
        return Ok(result);
    }
}