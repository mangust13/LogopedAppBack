using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgressService.Contracts;
using ProgressService.Domain;
using ProgressService.Infrastructure;

namespace ProgressService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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

    [HttpGet("child/{childProfileId}")]
    public async Task<IActionResult> GetByChild(int childProfileId)
    {
        var records = await _db.Records
            .Where(r => r.ChildProfileId == childProfileId)
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

    // Child Analytics
    [HttpGet("child/{childProfileId}/summary")]
    public async Task<IActionResult> GetChildSummary(int childProfileId)
    {
        var records = await _db.Records
            .Where(r => r.ChildProfileId == childProfileId)
            .ToListAsync();

        if (records.Count == 0)
            return Ok(new
            {
                childProfileId,
                totalAttempts = 0,
                averageAccuracy = 0,
                lastAttemptAt = (DateTime?)null
            });

        return Ok(new
        {
            childProfileId,
            totalAttempts = records.Count,
            averageAccuracy = Math.Round(records.Average(r => r.Accuracy), 2),
            lastAttemptAt = records.Max(r => r.CreatedAt)
        });
    }

    [HttpGet("child/{childProfileId}/exercise/{exerciseId}")]
    public async Task<IActionResult> GetExerciseProgress(int childProfileId, int exerciseId)
    {
        var records = await _db.Records
            .Where(r =>
                r.ChildProfileId == childProfileId &&
                r.ExerciseId == exerciseId)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();

        if (records.Count == 0)
            return NotFound();

        return Ok(new
        {
            exerciseId,
            attempts = records.Count,
            averageAccuracy = Math.Round(records.Average(r => r.Accuracy), 2),
            history = records.Select(r => new
            {
                r.Accuracy,
                r.CreatedAt
            })
        });
    }

    [HttpGet("child/{childProfileId}/last")]
    public async Task<IActionResult> GetLastAttempts(
    int childProfileId,
    [FromQuery] int limit = 10)
    {
        var records = await _db.Records
            .Where(r => r.ChildProfileId == childProfileId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(limit)
            .Select(r => new
            {
                r.ExerciseId,
                r.Accuracy,
                r.CreatedAt
            })
            .ToListAsync();

        return Ok(records);
    }

    //Logoped Analytics
    [HttpPost("logoped/summary")]
    public async Task<IActionResult> GetLogopedSummary([FromBody] int[] childProfileIds)
    {
        var records = await _db.Records
            .Where(r => childProfileIds.Contains(r.ChildProfileId))
            .ToListAsync();

        if (records.Count == 0)
            return Ok(new
            {
                totalChildren = childProfileIds.Length,
                totalAttempts = 0,
                averageAccuracy = 0,
                lastActivityAt = (DateTime?)null
            });

        return Ok(new
        {
            totalChildren = childProfileIds.Length,
            totalAttempts = records.Count,
            averageAccuracy = Math.Round(records.Average(r => r.Accuracy), 2),
            lastActivityAt = records.Max(r => r.CreatedAt)
        });
    }

    public class ProblemExercisesRequest
    {
        public int[] ChildProfileIds { get; set; } = [];
        public int MinAttempts { get; set; } = 5;
    }

    [HttpPost("logoped/problem-exercises")]
    public async Task<IActionResult> GetProblemExercises([FromBody] ProblemExercisesRequest req)
    {
        var result = await _db.Records
            .Where(r => req.ChildProfileIds.Contains(r.ChildProfileId))
            .GroupBy(r => r.ExerciseId)
            .Select(g => new
            {
                exerciseId = g.Key,
                attempts = g.Count(),
                averageAccuracy = g.Average(x => x.Accuracy)
            })
            .Where(x => x.attempts >= req.MinAttempts)
            .OrderBy(x => x.averageAccuracy)
            .Take(10)
            .ToListAsync();

        return Ok(result);
    }

    public class TrendRequest
    {
        public int[] ChildProfileIds { get; set; } = [];
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    [HttpPost("logoped/trends")]
    public async Task<IActionResult> GetTrends([FromBody] TrendRequest req)
    {
        var data = await _db.Records
            .Where(r =>
                req.ChildProfileIds.Contains(r.ChildProfileId) &&
                r.CreatedAt >= req.From &&
                r.CreatedAt <= req.To)
            .GroupBy(r => r.CreatedAt.Date)
            .Select(g => new
            {
                date = g.Key,
                averageAccuracy = Math.Round(g.Average(x => x.Accuracy), 2)
            })
            .OrderBy(x => x.date)
            .ToListAsync();

        return Ok(data);
    }


}
