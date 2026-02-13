using ExerciseService.Contracts;
using ExerciseService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        var list = await _db.Exercises
            .Select(x => new ExerciseDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                VideoUrl = x.VideoUrl,
                IconName = x.IconName,
                Category = x.Category
            })
            .ToListAsync();

        return Ok(list);
    }
}