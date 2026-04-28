using ExerciseService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExerciseService.Controllers;

[ApiController]
[Route("sound-cards")]
public class SoundCardsController : ControllerBase
{
    private readonly ExerciseDbContext _db;
    private readonly IWebHostEnvironment _env;

    public SoundCardsController(ExerciseDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> GetBySound([FromQuery] string sound)
    {
        if (string.IsNullOrWhiteSpace(sound))
            return BadRequest("sound is required");

        var cards = await _db.SoundCards
            .Include(c => c.Position)
            .Where(c => c.Sound == sound.ToLower())
            .OrderBy(c => c.Position.Code)
            .Select(c => new
            {
                c.Id,
                c.Sound,
                c.Word,
                c.ImageFile,
                Position = new
                {
                    c.Position.Code,
                    c.Position.DisplayName,
                },
                ImageUrl = $"/static/automation/{c.ImageFile}",
            })
            .ToListAsync();

        return Ok(cards);
    }
}