using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.Contracts;
using UserService.Infrastructure;

namespace UserService.Controllers;

[ApiController]
[Route("logoped")]
[Authorize]
public class LogopedController(UsersDbContext db) : ControllerBase
{
    [HttpGet("children")]
    [Authorize(Roles = "Logoped")]
    public async Task<IActionResult> GetMyChildren()
    {
        var logopedId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var children = await db.ChildAssignments
            .Where(x => x.LogopedUserId == logopedId)
            .Select(x => new GetChildProfilesDto
            {
                Id = x.ChildProfile.Id,
                Name = x.ChildProfile.Name,
                BirthDate = x.ChildProfile.BirthDate,
                ProblemSounds = x.ChildProfile.ProblemSounds,
                LogopedEmail = x.Logoped.Email
            })
            .ToListAsync();

        return Ok(children);
    }

    [HttpGet("logopeds")]
    public async Task<IActionResult> GetAllLogopeds()
    {
        var logopeds = await db.Users
            .Where(u => u.Role == "Logoped")
            .Select(u => new LogopedDto
            {
                Id = u.Id,
                Email = u.Email
            })
            .ToListAsync();

        return Ok(logopeds);
    }

    [HttpPut("children/{childId:int}")]
    [Authorize(Roles = "Logoped")]
    public async Task<IActionResult> UpdateChild(int childId, UpdateChildProfileDto dto)
    {
        var logopedId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var assignment = await db.ChildAssignments
            .Include(x => x.ChildProfile)
            .FirstOrDefaultAsync(x =>
                x.ChildProfileId == childId &&
                x.LogopedUserId == logopedId);

        if (assignment == null)
            return NotFound("Child not found");

        assignment.ChildProfile.Name = dto.Name;
        assignment.ChildProfile.BirthDate = dto.BirthDate;
        assignment.ChildProfile.ProblemSounds = dto.ProblemSounds ?? "";

        await db.SaveChangesAsync();

        return Ok();
    }
}