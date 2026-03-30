// Controllers/ComplexController.cs
using ExerciseService.Contracts;
using ExerciseService.Infrastructure;
using ExerciseService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ExerciseService.Domain;

namespace ExerciseService.Controllers;

[ApiController]
[Route("complexes")]
[Authorize]
public class ComplexController : ControllerBase
{
    private readonly ExerciseDbContext _db;
    private readonly IUserService _userService;
    private readonly ILogger<ComplexController> _logger;

    public ComplexController(
    ExerciseDbContext db,
    IUserService userService,
    ILogger<ComplexController> logger)
    {
        _db = db;
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ComplexDto>>> GetComplexes()
    {
        var logopedIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(logopedIdClaim) || !int.TryParse(logopedIdClaim, out var logopedId))
            return Unauthorized();

        if (userRole != "Logoped")
            return Forbid();

        var complexes = await _db.Complexes
            .Where(c => c.IsDefault || (c.LogopedId == logopedId && c.IsActive))
            .Include(c => c.Exercises)
                .ThenInclude(ci => ci.Exercise)
                    .ThenInclude(e => e.Tags)
                        .ThenInclude(t => t.Tag)
            .Select(c => new ComplexDto
            {
                Id = c.Id,
                Name = c.Name,
                DisplayName = c.DisplayName,
                Description = c.Description,
                LogopedId = c.LogopedId,
                IsDefault = c.IsDefault,
                CreatedAt = c.CreatedAt,
                IsActive = c.IsActive,
                ExerciseCount = c.Exercises.Count,
                Exercises = c.Exercises.OrderBy(ci => ci.Order).Select(ci => new ExerciseDto
                {
                    Id = ci.Exercise.Id,
                    Title = ci.Exercise.Title,
                    Description = ci.Exercise.Description,
                    VideoPath = ci.Exercise.VideoPath,
                    IconName = ci.Exercise.IconName,
                    Tags = ci.Exercise.Tags.Select(t => new ExerciseTagDto
                    {
                        Id = t.Tag.Id,
                        Name = t.Tag.Name,
                        Category = t.Tag.Category,
                        DisplayName = t.Tag.DisplayName
                    }).ToList()
                }).ToList()
            })
            .OrderBy(c => c.IsDefault ? 0 : 1)
            .ThenBy(c => c.CreatedAt)
            .ToListAsync();

        return Ok(complexes);
    }

    [HttpGet("public")]
    public async Task<ActionResult<List<ComplexDto>>> GetPublicComplexes()
    {
        var complexes = await _db.Complexes
            .Where(c => c.IsDefault)
            .Include(c => c.Exercises)
                .ThenInclude(ci => ci.Exercise)
                    .ThenInclude(e => e.Tags)
                        .ThenInclude(t => t.Tag)
            .Select(c => new ComplexDto
            {
                Id = c.Id,
                Name = c.Name,
                DisplayName = c.DisplayName,
                Description = c.Description,
                LogopedId = c.LogopedId,
                IsDefault = c.IsDefault,
                CreatedAt = c.CreatedAt,
                IsActive = c.IsActive,
                ExerciseCount = c.Exercises.Count,
                Exercises = c.Exercises.OrderBy(ci => ci.Order).Select(ci => new ExerciseDto
                {
                    Id = ci.Exercise.Id,
                    Title = ci.Exercise.Title,
                    Description = ci.Exercise.Description,
                    VideoPath = ci.Exercise.VideoPath,
                    IconName = ci.Exercise.IconName,
                    Tags = ci.Exercise.Tags.Select(t => new ExerciseTagDto
                    {
                        Id = t.Tag.Id,
                        Name = t.Tag.Name,
                        Category = t.Tag.Category,
                        DisplayName = t.Tag.DisplayName
                    }).ToList()
                }).ToList()
            })
            .ToListAsync();

        return Ok(complexes);
    }

    [HttpGet("public/{id}")]
    public async Task<ActionResult<ComplexDto>> GetPublicComplexById(int id)
    {
        var complex = await _db.Complexes
            .Include(c => c.Exercises)
                .ThenInclude(ci => ci.Exercise)
                    .ThenInclude(e => e.Tags)
                        .ThenInclude(t => t.Tag)
            .Where(c => c.Id == id && c.IsDefault)
            .Select(c => new ComplexDto
            {
                Id = c.Id,
                Name = c.Name,
                DisplayName = c.DisplayName,
                Description = c.Description,
                LogopedId = c.LogopedId,
                IsDefault = c.IsDefault,
                CreatedAt = c.CreatedAt,
                IsActive = c.IsActive,
                ExerciseCount = c.Exercises.Count,
                Exercises = c.Exercises.OrderBy(ci => ci.Order).Select(ci => new ExerciseDto
                {
                    Id = ci.Exercise.Id,
                    Title = ci.Exercise.Title,
                    Description = ci.Exercise.Description,
                    VideoPath = ci.Exercise.VideoPath,
                    IconName = ci.Exercise.IconName,
                    Tags = ci.Exercise.Tags.Select(t => new ExerciseTagDto
                    {
                        Id = t.Tag.Id,
                        Name = t.Tag.Name,
                        Category = t.Tag.Category,
                        DisplayName = t.Tag.DisplayName
                    }).ToList()
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (complex == null)
            return NotFound();

        return Ok(complex);
    }

    [HttpGet("assigned")]
    public async Task<ActionResult<List<ComplexDto>>> GetAssignedComplexes()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        if (userRole != "User")
            return Forbid();

        var childId = userId;

        var assignedComplexes = await _db.ComplexAssignments
            .Include(a => a.Complex)
                .ThenInclude(c => c.Exercises)
                    .ThenInclude(ci => ci.Exercise)
                        .ThenInclude(e => e.Tags)
                            .ThenInclude(t => t.Tag)
            .Where(a => a.ChildId == childId && a.IsActive)
            .Select(a => new ComplexDto
            {
                Id = a.Complex.Id,
                Name = a.Complex.Name,
                DisplayName = a.Complex.DisplayName,
                Description = a.Complex.Description,
                LogopedId = a.Complex.LogopedId,
                IsDefault = a.Complex.IsDefault,
                CreatedAt = a.Complex.CreatedAt,
                IsActive = a.Complex.IsActive,
                ExerciseCount = a.Complex.Exercises.Count,
                Exercises = a.Complex.Exercises.OrderBy(ci => ci.Order).Select(ci => new ExerciseDto
                {
                    Id = ci.Exercise.Id,
                    Title = ci.Exercise.Title,
                    Description = ci.Exercise.Description,
                    VideoPath = ci.Exercise.VideoPath,
                    IconName = ci.Exercise.IconName,
                    Tags = ci.Exercise.Tags.Select(t => new ExerciseTagDto
                    {
                        Id = t.Tag.Id,
                        Name = t.Tag.Name,
                        Category = t.Tag.Category,
                        DisplayName = t.Tag.DisplayName
                    }).ToList()
                }).ToList()
            })
            .ToListAsync();

        return Ok(assignedComplexes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ComplexDto>> GetComplexById(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var complex = await _db.Complexes
            .Include(c => c.Exercises)
                .ThenInclude(ci => ci.Exercise)
                    .ThenInclude(e => e.Tags)
                        .ThenInclude(t => t.Tag)
            .Where(c => c.Id == id)
            .Select(c => new ComplexDto
            {
                Id = c.Id,
                Name = c.Name,
                DisplayName = c.DisplayName,
                Description = c.Description,
                LogopedId = c.LogopedId,
                IsDefault = c.IsDefault,
                CreatedAt = c.CreatedAt,
                IsActive = c.IsActive,
                ExerciseCount = c.Exercises.Count,
                Exercises = c.Exercises.OrderBy(ci => ci.Order).Select(ci => new ExerciseDto
                {
                    Id = ci.Exercise.Id,
                    Title = ci.Exercise.Title,
                    Description = ci.Exercise.Description,
                    VideoPath = ci.Exercise.VideoPath,
                    IconName = ci.Exercise.IconName,
                    Tags = ci.Exercise.Tags.Select(t => new ExerciseTagDto
                    {
                        Id = t.Tag.Id,
                        Name = t.Tag.Name,
                        Category = t.Tag.Category,
                        DisplayName = t.Tag.DisplayName
                    }).ToList()
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (complex == null)
            return NotFound();

        if (userRole == "Logoped")
        {
            if (!complex.IsDefault && complex.LogopedId != userId)
                return Forbid();
        }
        else if (userRole == "User")
        {
            var hasAccess = await _db.ComplexAssignments
                .AnyAsync(a => a.ComplexId == id && a.ChildId == userId && a.IsActive);

            if (!hasAccess)
                return Forbid();
        }

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

        var exercises = await _db.Exercises
            .Where(e => request.ExerciseIds.Contains(e.Id))
            .ToListAsync();

        if (exercises.Count != request.ExerciseIds.Count)
            return BadRequest("Some exercises not found");

        var complex = new Complex
        {
            Name = request.Name,
            DisplayName = request.Name,
            Description = request.Description,
            LogopedId = logopedId,
            IsDefault = false
        };

        _db.Complexes.Add(complex);
        await _db.SaveChangesAsync();

        for (int i = 0; i < request.ExerciseIds.Count; i++)
        {
            var complexItem = new ComplexItem
            {
                ComplexId = complex.Id,
                ExerciseId = request.ExerciseIds[i],
                Order = i + 1
            };
            _db.ComplexItems.Add(complexItem);
        }

        await _db.SaveChangesAsync();

        return await GetComplexById(complex.Id);
    }

    // Controllers/ComplexController.cs в ExerciseService
    [HttpPost("{complexId}/assign")]
    public async
     Task<ActionResult> AssignComplexToChildren(int complexId, [FromBody] AssignComplexRequest request)
    {
        var logopedIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        _logger.LogInformation("AssignComplexToChildren called. ComplexId: {ComplexId}, LogopedId: {LogopedId}, ChildIds: {ChildIds}",
            complexId, logopedIdClaim, string.Join(", ", request.ChildIds));

        if (string.IsNullOrEmpty(logopedIdClaim) || !int.TryParse(logopedIdClaim, out var logopedId))
        {
            _logger.LogWarning("Unauthorized: Invalid logopedId claim");
            return Unauthorized();
        }

        if (userRole != "Logoped")
        {
            _logger.LogWarning("Forbidden: User role is not Logoped. Actual role: {Role}", userRole);
            return Forbid();
        }

        if (!request.ChildIds.Any())
        {
            _logger.LogWarning("Bad request: No child IDs provided");
            return BadRequest("At least one child is required");
        }

        var complex = await _db.Complexes
            .FirstOrDefaultAsync(c => c.Id == complexId && (c.IsDefault || c.LogopedId == logopedId));

        if (complex == null)
        {
            _logger.LogWarning("Not found: Complex not found or access denied. ComplexId: {ComplexId}, LogopedId: {LogopedId}",
                complexId, logopedId);
            return NotFound("Complex not found or access denied");
        }

        _logger.LogInformation("Complex found. Name: {Name}, IsDefault: {IsDefault}", complex.Name, complex.IsDefault);

        // Отримуємо всіх дітей логопеда
        var logopedChildren = await _userService.GetLogopedChildren(logopedId);
        _logger.LogInformation("Retrieved {Count} children for logoped {LogopedId}", logopedChildren.Count, logopedId);

        var logopedChildIds = logopedChildren.Select(c => c.Id).ToList();
        _logger.LogInformation("Logoped child IDs: {ChildIds}", string.Join(", ", logopedChildIds));

        // Перевіряємо, чи всі діти з запиту призначені логопеду
        var invalidChildIds = request.ChildIds.Where(id => !logopedChildIds.Contains(id)).ToList();

        if (invalidChildIds.Any())
        {
            _logger.LogWarning("Bad request: Some children are not assigned to this logoped. Invalid child IDs: {InvalidChildIds}",
                string.Join(", ", invalidChildIds));
            return BadRequest($"Children with IDs {string.Join(", ", invalidChildIds)} are not assigned to this logoped");
        }

        _logger.LogInformation("All children are assigned to this logoped. Proceeding with assignment.");

        foreach (var childId in request.ChildIds)
        {
            var existingAssignment = await _db.ComplexAssignments
                .FirstOrDefaultAsync(a => a.ComplexId == complexId && a.ChildId == childId && a.IsActive);

            if (existingAssignment == null)
            {
                _logger.LogInformation("Creating new assignment for child {ChildId} and complex {ComplexId}", childId, complexId);
                var assignment = new ComplexAssignment
                {
                    ComplexId = complexId,
                    ChildId = childId
                };
                _db.ComplexAssignments.Add(assignment);
            }
            else
            {
                _logger.LogInformation("Assignment already exists for child {ChildId} and complex {ComplexId}", childId, complexId);
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Complex assigned successfully");
        return Ok(new { message = "Complex assigned successfully" });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ComplexDto>> UpdateComplex(int id, [FromBody] CreateComplexRequest request)
    {
        var logopedIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(logopedIdClaim) || !int.TryParse(logopedIdClaim, out var logopedId))
            return Unauthorized();

        if (userRole != "Logoped")
            return Forbid();

        var complex = await _db.Complexes
            .Include(c => c.Exercises)
            .FirstOrDefaultAsync(c => c.Id == id && (c.IsDefault || c.LogopedId == logopedId));

        if (complex == null)
            return NotFound();

        complex.Name = request.Name;
        complex.DisplayName = request.Name;
        complex.Description = request.Description;

        var oldItems = await _db.ComplexItems.Where(ci => ci.ComplexId == id).ToListAsync();
        _db.ComplexItems.RemoveRange(oldItems);

        for (int i = 0; i < request.ExerciseIds.Count; i++)
        {
            _db.ComplexItems.Add(new ComplexItem
            {
                ComplexId = id,
                ExerciseId = request.ExerciseIds[i],
                Order = i + 1
            });
        }

        await _db.SaveChangesAsync();

        return await GetComplexById(id);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteComplex(int id)
    {
        var logopedIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(logopedIdClaim) || !int.TryParse(logopedIdClaim, out var logopedId))
            return Unauthorized();

        if (userRole != "Logoped")
            return Forbid();

        var complex = await _db.Complexes
            .FirstOrDefaultAsync(c => c.Id == id && c.LogopedId == logopedId && !c.IsDefault);

        if (complex == null)
            return NotFound("Complex not found or cannot be deleted");

        complex.IsActive = false;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Complex deleted successfully" });
    }
}