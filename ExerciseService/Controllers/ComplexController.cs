using ExerciseService.Contracts;
using ExerciseService.Domain;
using ExerciseService.Infrastructure;
using ExerciseService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExerciseService.Controllers;

[ApiController]
[Route("complexes")]
[Authorize]
public class ComplexController(
    ExerciseDbContext db,
    IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ComplexDto>>> GetComplexes()
    {
        var logopedIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(logopedIdClaim) || !int.TryParse(logopedIdClaim, out var logopedId))
            return Unauthorized();

        if (userRole != "Logoped")
            return Forbid();

        var complexes = await db.Complexes
            .AsNoTracking()
            .Where(c => c.IsActive && (c.IsDefault || c.LogopedId == logopedId))
            .OrderBy(c => c.IsDefault ? 0 : 1)
            .ThenBy(c => c.CreatedAt)
            .Select(ComplexDto.FromEntity)
            .ToListAsync();

        return Ok(complexes);
    }

    [HttpGet("assigned")]
    public async Task<ActionResult<List<ComplexDto>>> GetAssignedComplexes()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out _))
            return Unauthorized();

        if (userRole != "User")
            return Forbid();

        var children = await userService.GetMyChildren();
        var childIds = children.Select(c => c.Id).ToList();

        if (!childIds.Any())
            return Ok(new List<ComplexDto>());

        var assignedComplexes = await db.Complexes
            .AsNoTracking()
            .Where(c => c.IsActive && c.Assignments.Any(a => childIds.Contains(a.ChildId) && a.IsActive))
            .OrderBy(c => c.CreatedAt)
            .Select(ComplexDto.FromEntity)
            .ToListAsync();

        return Ok(assignedComplexes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ComplexDto>> GetComplexById(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        IQueryable<Complex> query = db.Complexes
            .AsNoTracking()
            .Where(c => c.Id == id && c.IsActive);

        if (userRole == "Logoped")
        {
            query = query.Where(c => c.IsDefault || c.LogopedId == userId);
        }
        else if (userRole == "User")
        {
            var children = await userService.GetMyChildren();
            var childIds = children.Select(c => c.Id).ToList();

            query = query.Where(c => c.Assignments.Any(a => childIds.Contains(a.ChildId) && a.IsActive));
        }
        else
        {
            return Forbid();
        }

        var complex = await query
            .Select(ComplexDto.FromEntity)
            .FirstOrDefaultAsync();

        if (complex == null)
            return NotFound();

        return Ok(complex);
    }

    [HttpPost]
    public async Task<ActionResult<ComplexDto>> CreateComplex([FromBody] CreateComplexRequest request)
    {
        var logopedIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(logopedIdClaim) || !int.TryParse(logopedIdClaim, out var logopedId))
            return Unauthorized();

        if (userRole != "Logoped")
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Complex name is required");

        if (!request.ExerciseIds.Any())
            return BadRequest("At least one exercise is required");

        var exercisesCount = await db.Exercises
            .CountAsync(e => request.ExerciseIds.Contains(e.Id));

        if (exercisesCount != request.ExerciseIds.Count)
            return BadRequest("Some exercises not found");

        var complex = new Complex
        {
            Name = request.Name,
            DisplayName = request.Name,
            Description = request.Description,
            FolderName = "",
            LogopedId = logopedId,
            IsDefault = false,
            IsActive = true
        };

        db.Complexes.Add(complex);
        await db.SaveChangesAsync();

        for (var i = 0; i < request.ExerciseIds.Count; i++)
        {
            db.ComplexItems.Add(new ComplexItem
            {
                ComplexId = complex.Id,
                ExerciseId = request.ExerciseIds[i],
                Order = i + 1
            });
        }

        await db.SaveChangesAsync();

        return await GetComplexById(complex.Id);
    }

    [HttpPost("{complexId:int}/assign")]
    public async Task<ActionResult> AssignComplexToChildren(int complexId, [FromBody] AssignComplexRequest request)
    {
        var logopedIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(logopedIdClaim) || !int.TryParse(logopedIdClaim, out var logopedId))
            return Unauthorized();

        if (userRole != "Logoped")
            return Forbid();

        var complex = await db.Complexes
            .FirstOrDefaultAsync(c => c.Id == complexId && c.IsActive && (c.IsDefault || c.LogopedId == logopedId));

        if (complex == null)
            return NotFound("Complex not found or access denied");

        var selectedChildIds = request.ChildIds.Distinct().ToList();

        var logopedChildren = await userService.GetLogopedChildren(logopedId);
        var logopedChildIds = logopedChildren.Select(c => c.Id).ToList();

        var invalidChildIds = selectedChildIds
            .Where(id => !logopedChildIds.Contains(id))
            .ToList();

        if (invalidChildIds.Any())
            return BadRequest($"Children with IDs {string.Join(", ", invalidChildIds)} are not assigned to this logoped");

        var existingAssignments = await db.ComplexAssignments
            .Where(a => a.ComplexId == complexId && logopedChildIds.Contains(a.ChildId))
            .ToListAsync();

        foreach (var assignment in existingAssignments)
        {
            assignment.IsActive = selectedChildIds.Contains(assignment.ChildId);
            assignment.CompletedAt = null;
        }

        var existingChildIds = existingAssignments.Select(a => a.ChildId).ToList();

        foreach (var childId in selectedChildIds)
        {
            if (existingChildIds.Contains(childId))
                continue;

            db.ComplexAssignments.Add(new ComplexAssignment
            {
                ComplexId = complexId,
                ChildId = childId,
                IsActive = true
            });
        }

        await db.SaveChangesAsync();

        return Ok(new { message = "Complex assignments updated successfully" });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ComplexDto>> UpdateComplex(int id, [FromBody] CreateComplexRequest request)
    {
        var logopedIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(logopedIdClaim) || !int.TryParse(logopedIdClaim, out var logopedId))
            return Unauthorized();

        if (userRole != "Logoped")
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Complex name is required");

        if (!request.ExerciseIds.Any())
            return BadRequest("At least one exercise is required");

        var complex = await db.Complexes
            .FirstOrDefaultAsync(c => c.Id == id && c.LogopedId == logopedId && !c.IsDefault && c.IsActive);

        if (complex == null)
            return NotFound();

        var exercisesCount = await db.Exercises
            .CountAsync(e => request.ExerciseIds.Contains(e.Id));

        if (exercisesCount != request.ExerciseIds.Count)
            return BadRequest("Some exercises not found");

        complex.Name = request.Name;
        complex.DisplayName = request.Name;
        complex.Description = request.Description;

        var oldItems = await db.ComplexItems
            .Where(ci => ci.ComplexId == id)
            .ToListAsync();

        db.ComplexItems.RemoveRange(oldItems);

        for (var i = 0; i < request.ExerciseIds.Count; i++)
        {
            db.ComplexItems.Add(new ComplexItem
            {
                ComplexId = id,
                ExerciseId = request.ExerciseIds[i],
                Order = i + 1
            });
        }

        await db.SaveChangesAsync();

        return await GetComplexById(id);
    }

    [HttpGet("{complexId:int}/assigned-children")]
    public async Task<ActionResult<List<int>>> GetAssignedChildren(int complexId)
    {
        var logopedIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(logopedIdClaim) || !int.TryParse(logopedIdClaim, out var logopedId))
            return Unauthorized();

        if (userRole != "Logoped")
            return Forbid();

        var complexExists = await db.Complexes
            .AnyAsync(c => c.Id == complexId && c.IsActive && (c.IsDefault || c.LogopedId == logopedId));

        if (!complexExists)
            return NotFound("Complex not found or access denied");

        var logopedChildren = await userService.GetLogopedChildren(logopedId);
        var logopedChildIds = logopedChildren.Select(c => c.Id).ToList();

        var assignedChildIds = await db.ComplexAssignments
            .Where(a =>
                a.ComplexId == complexId &&
                a.IsActive &&
                logopedChildIds.Contains(a.ChildId))
            .Select(a => a.ChildId)
            .ToListAsync();

        return Ok(assignedChildIds);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteComplex(int id)
    {
        var logopedIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(logopedIdClaim) || !int.TryParse(logopedIdClaim, out var logopedId))
            return Unauthorized();

        if (userRole != "Logoped")
            return Forbid();

        var complex = await db.Complexes
            .FirstOrDefaultAsync(c => c.Id == id && c.LogopedId == logopedId && !c.IsDefault && c.IsActive);

        if (complex == null)
            return NotFound("Complex not found or cannot be deleted");

        complex.IsActive = false;
        await db.SaveChangesAsync();

        return Ok(new { message = "Complex deleted successfully" });
    }
}