using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.Contracts;
using UserService.Domain;
using UserService.Infrastructure;

namespace UserService.Controllers;

[ApiController]
[Route("api/children")]
[Authorize]
public class ChildrenController : ControllerBase
{
    private readonly UsersDbContext _db;

    public ChildrenController(UsersDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateChildProfileDto dto)
    {
        var parentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var child = new ChildProfile
        {
            ParentUserId = parentId,
            Name = dto.Name,
            BirthDate = dto.BirthDate,
            ProblemSounds = dto.ProblemSounds
        };

        _db.ChildProfiles.Add(child);
        await _db.SaveChangesAsync();

        return Ok(child.Id);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyChildren()
    {
        var parentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var children = await _db.ChildProfiles
            .Where(c => c.ParentUserId == parentId)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.BirthDate,
                c.ProblemSounds
            })
            .ToListAsync();

        return Ok(children);
    }

    [HttpPost("{childId}/assign-logoped")]
    public async Task<IActionResult> AssignLogoped(int childId, AssignLogopedDto dto)
    {
        var parentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var child = await _db.ChildProfiles
            .FirstOrDefaultAsync(c =>
                c.Id == childId &&
                c.ParentUserId == parentId);

        if (child == null)
            return NotFound("Child not found");

        var logoped = await _db.Users
            .FirstOrDefaultAsync(u =>
                u.Email == dto.LogopedEmail &&
                u.Role == "Logoped");

        if (logoped == null)
            return NotFound("Logoped not found");

        var exists = await _db.ChildAssignments
            .AnyAsync(x =>
                x.ChildProfileId == childId &&
                x.LogopedUserId == logoped.Id);

        if (exists)
            return BadRequest("Already assigned");

        _db.ChildAssignments.Add(new ChildAssignment
        {
            ChildProfileId = childId,
            LogopedUserId = logoped.Id
        });

        await _db.SaveChangesAsync();
        return Ok();
    }

}
