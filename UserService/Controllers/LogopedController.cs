using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.Domain;
using UserService.Infrastructure;

namespace UserService.Controllers;

[ApiController]
[Route("logoped")]
[Authorize]
public class LogopedController : ControllerBase
{
    private readonly UsersDbContext _db;

    public LogopedController(UsersDbContext db)
    {
        _db = db;
    }

    [HttpGet("children")]
    public async Task<IActionResult> GetMyChildren()
    {
        var logopedId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var children = await _db.ChildAssignments
            .Where(x => x.LogopedUserId == logopedId)
            .Select(x => new
            {
                x.ChildProfile.Id,
                x.ChildProfile.Name,
                x.ChildProfile.BirthDate
            })
            .ToListAsync();

        return Ok(children);
    }

    [HttpGet("logopeds")]
    public async Task<IActionResult> GetAllLogopeds()
    {
        var logopeds = await _db.Users
            .Where(u => u.Role == "Logoped")
            .Select(u => new
            {
                u.Id,
                u.Email
            })
            .ToListAsync();

        return Ok(logopeds);
    }
}
