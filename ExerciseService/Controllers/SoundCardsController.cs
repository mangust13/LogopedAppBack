using ExerciseService.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExerciseService.Controllers;

[ApiController]
[Route("sound-cards")]
public class SoundCardsController(ExerciseDbContext db) : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetBySound([FromQuery] string sound)
    {
        if (string.IsNullOrWhiteSpace(sound))
            return BadRequest("sound is required");

        var cards = await db.SoundCards
            .Include(c => c.Position)
            .Where(c => c.Sound == sound.ToLower())
            .OrderBy(c => c.Position.Code)
            .Select(c => new
            {
                c.Id,
                c.Sound,
                c.Word,
                c.ImageFile,
                c.IsAlive,
                Position = new
                {
                    c.Position.Code,
                    c.Position.DisplayName,
                },
                ImageUrl = $"/static/automation/sound-{MapSound(c.Sound)}/{c.ImageFile}",
            })
            .ToListAsync();

        return Ok(cards);
    }

    private static string MapSound(string sound) => sound switch
    {
        "р" => "r",
        "л" => "l",
        "с" => "s",
        "ш" => "sh",
        "ж" => "zh",
        "з" => "z",
        "ч" => "ch",
        "ц" => "ts",
        "дж" => "dzh",
        "дз" => "dz",
        _ => sound
    };
}