using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgressService.Contracts;
using ProgressService.Domain;
using ProgressService.Infrastructure;

namespace ProgressService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProgressController : ControllerBase
{
    private readonly ProgressDbContext _db;

    public ProgressController(ProgressDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProgressCreateDto dto)
    {
        var record = new ProgressRecord
        {
            UserId = dto.UserId,
            ExerciseId = dto.ExerciseId,
            Accuracy = dto.Accuracy,
            Feedback = dto.Feedback,
            RecognizedText = dto.RecognizedText
        };

        _db.Records.Add(record);
        await _db.SaveChangesAsync();

        return Ok(record);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var records = await _db.Records
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(records);
    }

    [HttpGet("record/{id}")]
    public async Task<IActionResult> GetRecord(int id)
    {
        var record = await _db.Records.FindAsync(id);
        if (record == null) return NotFound();

        return Ok(record);
    }
}
