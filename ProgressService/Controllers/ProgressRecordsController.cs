using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgressService.Contracts;
using ProgressService.Domain;
using ProgressService.Infrastructure;

namespace ProgressService.Controllers;

[ApiController]
[Route("records")]
[Authorize]
public class ProgressRecordsController : ControllerBase
{
    private readonly ProgressDbContext _db;

    public ProgressRecordsController(ProgressDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProgressCreateDto dto)
    {
        var record = new ProgressRecord
        {
            ChildProfileId = dto.ChildProfileId,
            ExerciseId = dto.ExerciseId,
            Accuracy = dto.Accuracy,
            Feedback = dto.Feedback,
            RecognizedText = dto.RecognizedText
        };

        _db.Records.Add(record);
        await _db.SaveChangesAsync();

        return Ok(record);
    }

    [HttpGet("child/{childId}")]
    public async Task<IActionResult> GetByChild(int childId)
    {
        var records = await _db.Records
            .Where(r => r.ChildProfileId == childId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(records);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var record = await _db.Records.FindAsync(id);
        if (record == null)
            return NotFound();

        return Ok(record);
    }
}
