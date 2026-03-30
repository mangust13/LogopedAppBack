using ExerciseService.Contracts;
using ExerciseService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExerciseService.Controllers;

[ApiController]
[Route("")]
public class ExercisesController : ControllerBase
{
    private readonly ExerciseDbContext _db;

    public ExercisesController(ExerciseDbContext db)
    {
        _db = db;
    }

    [HttpGet("/all")]
    public async Task<ActionResult<List<ExerciseDto>>> GetAll()
    {
        var exercises = await _db.Exercises
            .Include(x => x.Tags)
                .ThenInclude(t => t.Tag)
            .Select(x => new ExerciseDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                VideoPath = x.VideoPath,
                IconName = x.IconName,
                Tags = x.Tags.Select(t => new ExerciseTagDto
                {
                    Id = t.Tag.Id,
                    Name = t.Tag.Name,
                    Category = t.Tag.Category,
                    DisplayName = t.Tag.DisplayName
                }).ToList()
            })
            .ToListAsync();

        return Ok(exercises);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExerciseDto>> GetById(int id)
    {
        var exercise = await _db.Exercises
            .Include(x => x.Tags)
                .ThenInclude(t => t.Tag)
            .Where(x => x.Id == id)
            .Select(x => new ExerciseDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                VideoPath = x.VideoPath,
                IconName = x.IconName,
                Tags = x.Tags.Select(t => new ExerciseTagDto
                {
                    Id = t.Tag.Id,
                    Name = t.Tag.Name,
                    Category = t.Tag.Category,
                    DisplayName = t.Tag.DisplayName
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (exercise == null)
            return NotFound();

        return Ok(exercise);
    }

    [HttpGet("tags")]
    public async Task<ActionResult<List<ExerciseTagDto>>> GetTags()
    {
        var tags = await _db.ExerciseTags
            .Select(t => new ExerciseTagDto
            {
                Id = t.Id,
                Name = t.Name,
                Category = t.Category,
                DisplayName = t.DisplayName
            })
            .ToListAsync();

        return Ok(tags);
    }

    [HttpGet("tags/{category}")]
    public async Task<ActionResult<List<ExerciseTagDto>>> GetTagsByCategory(string category)
    {
        var tags = await _db.ExerciseTags
            .Where(t => t.Category == category)
            .Select(t => new ExerciseTagDto
            {
                Id = t.Id,
                Name = t.Name,
                Category = t.Category,
                DisplayName = t.DisplayName
            })
            .ToListAsync();

        return Ok(tags);
    }
}