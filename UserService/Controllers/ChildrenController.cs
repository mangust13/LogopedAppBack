using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UserService.Contracts;
using UserService.Domain;
using UserService.Infrastructure;

namespace UserService.Controllers;

[ApiController]
[Route("children")]
[Authorize]
public class ChildrenController(UsersDbContext db) : ControllerBase
{
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

        db.ChildProfiles.Add(child);
        await db.SaveChangesAsync();

        return Ok(child.Id);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyChildren()
    {
        var parentId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var children = await db.ChildProfiles
            .Where(c => c.ParentUserId == parentId)
            .Select(c => new GetChildProfilesDto
            {
                Id = c.Id,
                Name = c.Name,
                BirthDate = c.BirthDate,
                ProblemSounds = c.ProblemSounds,
                LogopedEmail = db.ChildAssignments
                    .Where(a => a.ChildProfileId == c.Id)
                    .Select(a => a.Logoped.Email)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(children);
    }


    [HttpPut("{childId}")]
    public async Task<IActionResult> UpdateChild(int childId, UpdateChildProfileDto dto)
    {
        var parentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var child = await db.ChildProfiles
            .FirstOrDefaultAsync(c => c.Id == childId && c.ParentUserId == parentId);

        if (child == null)
            return NotFound("Child not found");

        child.Name = dto.Name;
        child.BirthDate = dto.BirthDate;
        child.ProblemSounds = dto.ProblemSounds;

        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{childId}")]
    public async Task<IActionResult> DeleteChild(int childId)
    {
        var parentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var child = await db.ChildProfiles
            .FirstOrDefaultAsync(c => c.Id == childId && c.ParentUserId == parentId);

        if (child == null)
            return NotFound("Child not found");

        var assignments = db.ChildAssignments.Where(a => a.ChildProfileId == childId);
        db.ChildAssignments.RemoveRange(assignments);

        db.ChildProfiles.Remove(child);
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("{childId}/assign-logoped")]
    public async Task<IActionResult> AssignLogoped(int childId, AssignLogopedDto dto)
    {
        var parentId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        var child = await db.ChildProfiles
            .FirstOrDefaultAsync(c =>
                c.Id == childId &&
                c.ParentUserId == parentId);

        if (child == null)
            return NotFound("Child not found");

        var logoped = await db.Users
            .FirstOrDefaultAsync(u =>
                u.Email == dto.LogopedEmail &&
                u.Role == "Logoped");

        if (logoped == null)
            return NotFound("Logoped not found");

        var existingAssignment = await db.ChildAssignments
            .FirstOrDefaultAsync(x =>
                x.ChildProfileId == childId);

        if (existingAssignment != null)
        {
            existingAssignment.LogopedUserId = logoped.Id;
        }
        else
        {
            db.ChildAssignments.Add(new ChildAssignment
            {
                ChildProfileId = childId,
                LogopedUserId = logoped.Id
            });
        }

        await db.SaveChangesAsync();
        return Ok();
    }
}
