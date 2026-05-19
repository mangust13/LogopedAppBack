using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgressService.Contracts;
using ProgressService.Services;

namespace ProgressService.Controllers;

[ApiController]
[Route("activity")]
[Authorize]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _service;

    public ActivityController(IActivityService service)
    {
        _service = service;
    }

    [HttpPost("track")]
    public async Task<IActionResult> Track([FromBody] TrackActivityDto dto)
    {
        await _service.TrackAsync(dto.ChildId, dto.ActivityType);
        return Ok();
    }

    [HttpPost("inactive")]
    public async Task<IActionResult> GetInactiveChildren([FromBody] InactiveChildrenRequestDto dto)
    {
        var result = await _service.GetInactiveChildrenAsync(dto.ChildIds, dto.ThresholdDays);
        return Ok(result);
    }

    [HttpGet("streak")]
    public async Task<IActionResult> GetStreak([FromQuery] int childId)
    {
        var result = await _service.GetStreakAsync(childId);
        return Ok(result);
    }

    [HttpGet("dates")]
    public async Task<IActionResult> GetActiveDates([FromQuery] int childId, [FromQuery] int days = 7)
    {
        var result = await _service.GetActiveDatesAsync(childId, days);
        return Ok(result);
    }
}