using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgressService.Contracts;
using ProgressService.Services;
using System.Security.Claims;

namespace ProgressService.Controllers;

[ApiController]
[Route("sessions")]
[Authorize]
public class LogopedSessionController : ControllerBase
{
    private readonly ILogopedSessionService _service;

    public LogopedSessionController(ILogopedSessionService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize(Roles = "Logoped")]
    public async Task<IActionResult> Create([FromBody] CreateSessionDto dto)
    {
        var logopedId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.CreateAsync(logopedId, dto);
        return Ok(result);
    }

    [HttpPut("{sessionId:int}")]
    [Authorize(Roles = "Logoped")]
    public async Task<IActionResult> Update(int sessionId, [FromBody] UpdateSessionDto dto)
    {
        var logopedId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            var result = await _service.UpdateAsync(logopedId, sessionId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{sessionId:int}")]
    [Authorize(Roles = "Logoped")]
    public async Task<IActionResult> Delete(int sessionId)
    {
        var logopedId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            await _service.DeleteAsync(logopedId, sessionId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("my")]
    [Authorize(Roles = "Logoped")]
    public async Task<IActionResult> GetMySessions([FromQuery] int? childId)
    {
        var logopedId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _service.GetByLogopedAsync(logopedId, childId);
        return Ok(result);
    }

    [HttpGet("child/{childId:int}")]
    public async Task<IActionResult> GetByChild(int childId)
    {
        var result = await _service.GetByChildAsync(childId);
        return Ok(result);
    }
}