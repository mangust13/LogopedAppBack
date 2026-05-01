using ExerciseService.Contracts;
using ExerciseService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExerciseService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExercisesController(ExerciseDbContext db) : ControllerBase
{
    [HttpGet("all")]
    public async Task<ActionResult<List<ExerciseDto>>> GetAll([FromQuery] string? sound = null)
    {
        var query = db.Exercises
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(sound))
        {
            var tagName = $"sound-{sound.ToLower()}";
            query = query.Where(x => x.Tags.Any(t => t.Tag.Name == tagName));
        }

        var exercises = await query
            .Select(ExerciseDto.From)
            .ToListAsync();

        return Ok(exercises);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExerciseDto>> GetById(int id)
    {
        var exercise = await db.Exercises
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(ExerciseDto.From)
            .FirstOrDefaultAsync();

        if (exercise == null)
            return NotFound();

        return Ok(exercise);
    }

    [HttpGet("tags")]
    public async Task<ActionResult<List<ExerciseTagDto>>> GetTags()
    {
        var tags = await db.ExerciseTags
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
        var tags = await db.ExerciseTags
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