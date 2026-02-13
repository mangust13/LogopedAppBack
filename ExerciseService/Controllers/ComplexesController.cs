using ExerciseService.Contracts;
using ExerciseService.Domain;
using ExerciseService.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("complexes")]
public class ComplexesController : ControllerBase
{
    private readonly ExerciseDbContext _db;

    public ComplexesController(ExerciseDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    [Authorize(Roles = "Logoped")]
    public async Task<IActionResult> Create([FromBody] CreateComplexDto dto)
    {
        var logopedId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var complex = new ExerciseComplex
        {
            Title = dto.Title,
            LogopedId = logopedId
        };

        _db.Complexes.Add(complex);
        await _db.SaveChangesAsync();

        var items = dto.ExerciseIds.Select((exId, index) => new ComplexItem
        {
            ComplexId = complex.Id,
            ExerciseId = exId,
            Order = index
        });

        _db.ComplexItems.AddRange(items);
        await _db.SaveChangesAsync();

        return Ok(new { complex.Id });
    }

    [HttpGet("my")]
    [Authorize(Roles = "Logoped")]
    public async Task<ActionResult<List<ComplexDto>>> GetMyComplexes()
    {
        var logopedId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var complexes = await _db.Complexes
            .Where(c => c.LogopedId == logopedId)
            .Include(c => c.Items)
            .ThenInclude(i => i.Exercise)
            .Select(c => new ComplexDto
            {
                Id = c.Id,
                Title = c.Title,
                Exercises = c.Items.OrderBy(i => i.Order).Select(i => new ExerciseDto
                {
                    Id = i.Exercise.Id,
                    Title = i.Exercise.Title,
                    Description = i.Exercise.Description,
                    VideoUrl = i.Exercise.VideoUrl,
                    IconName = i.Exercise.IconName,
                    Category = i.Exercise.Category
                }).ToList()
            })
            .ToListAsync();

        return Ok(complexes);
    }

    [HttpPost("assign")]
    [Authorize(Roles = "Logoped")]
    public async Task<IActionResult> Assign([FromBody] AssignHomeworkDto dto)
    {
        var homework = new ChildHomework
        {
            ChildProfileId = dto.ChildId,
            ComplexId = dto.ComplexId
        };

        _db.Homeworks.Add(homework);
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("child/{childId}")]
    public async Task<ActionResult<List<ComplexDto>>> GetChildHomework(int childId)
    {
        var homeworks = await _db.Homeworks
            .Where(h => h.ChildProfileId == childId && !h.IsCompleted)
            .Include(h => h.Complex)
            .ThenInclude(c => c.Items)
            .ThenInclude(i => i.Exercise)
            .OrderByDescending(h => h.AssignedAt)
            .Select(h => new ComplexDto
            {
                Id = h.Complex.Id,
                Title = h.Complex.Title,
                Exercises = h.Complex.Items.OrderBy(i => i.Order).Select(i => new ExerciseDto
                {
                    Id = i.Exercise.Id,
                    Title = i.Exercise.Title,
                    Description = i.Exercise.Description,
                    VideoUrl = i.Exercise.VideoUrl,
                    IconName = i.Exercise.IconName,
                    Category = i.Exercise.Category
                }).ToList()
            })
            .ToListAsync();

        return Ok(homeworks);
    }
}