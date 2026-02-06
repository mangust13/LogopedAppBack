using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.Contracts;
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
            .Select(x => new LogopedChildDto
            {
                Id = x.ChildProfile.Id,
                Name = x.ChildProfile.Name,
                BirthDate = x.ChildProfile.BirthDate,
                ProblemSounds = x.ChildProfile.ProblemSounds
            })
            .ToListAsync();

        return Ok(children);
    }

    [HttpGet("logopeds")]
    public async Task<IActionResult> GetAllLogopeds()
    {
        var logopeds = await _db.Users
            .Where(u => u.Role == "Logoped")
            .Select(u => new LogopedDto
            {
                Id = u.Id,
                Email = u.Email
            })
            .ToListAsync();

        return Ok(logopeds);
    }
}
