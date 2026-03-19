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

    [HttpGet]
    public async Task<ActionResult<List<ExerciseDto>>> GetAll()
    {
        var exercises = await _db.Exercises
            .Include(x => x.MainCategory)
            .Include(x => x.Tags)
                .ThenInclude(t => t.Tag)
            .Select(x => new ExerciseDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                VideoPath = x.VideoPath,
                IconName = x.IconName,
                MainCategory = x.MainCategory.Name,
                MainCategoryDisplayName = x.MainCategory.DisplayName,
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

    [HttpGet("categories")]
    public async Task<ActionResult<List<ExerciseMainCategoryDto>>> GetMainCategories()
    {
        var categories = await _db.ExerciseMainCategories
            .Select(c => new ExerciseMainCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                DisplayName = c.DisplayName,
                FolderName = c.FolderName,
                ExerciseCount = c.Exercises.Count
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("categories/{categoryName}")]
    public async Task<ActionResult<List<ExerciseDto>>> GetByCategory(string categoryName)
    {
        var exercises = await _db.Exercises
            .Include(x => x.MainCategory)
            .Include(x => x.Tags)
                .ThenInclude(t => t.Tag)
            .Where(x => x.MainCategory.Name == categoryName || categoryName == "all")
            .Select(x => new ExerciseDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                VideoPath = x.VideoPath,
                IconName = x.IconName,
                MainCategory = x.MainCategory.Name,
                MainCategoryDisplayName = x.MainCategory.DisplayName,
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

    [HttpGet("{id}")]
    public async Task<ActionResult<ExerciseDto>> GetById(int id)
    {
        var exercise = await _db.Exercises
            .Include(x => x.MainCategory)
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
                MainCategory = x.MainCategory.Name,
                MainCategoryDisplayName = x.MainCategory.DisplayName,
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
}