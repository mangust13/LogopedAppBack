using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgressService.Contracts;
using ProgressService.Infrastructure;

namespace ProgressService.Controllers;

[ApiController]
[Route("")]
[Authorize]
public class ProgressAnalyticsController : ControllerBase
{
    private readonly ProgressDbContext _db;

    public ProgressAnalyticsController(ProgressDbContext db)
    {
        _db = db;
    }

    [HttpGet("child/{childId}/summary")]
    public async Task<ActionResult<ProgressSummaryDto>> GetChildSummary(int childId)
    {
        var records = await _db.Records
            .Where(r => r.ChildProfileId == childId)
            .ToListAsync();

        if (records.Count == 0)
        {
            return Ok(new ProgressSummaryDto
            {
                ChildId = childId,
                TotalAttempts = 0,
                AvgAccuracy = 0,
                LastActivityAt = null
            });
        }

        return Ok(new ProgressSummaryDto
        {
            ChildId = childId,
            TotalAttempts = records.Count,
            AvgAccuracy = Math.Round(records.Average(r => r.Accuracy), 2),
            LastActivityAt = records.Max(r => r.CreatedAt)
        });
    }

    [HttpGet("child/{childId}/last")]
    public async Task<ActionResult<List<ProgressAttemptDto>>> GetLastAttempts(
        int childId,
        [FromQuery] int limit = 10)
    {
        var attempts = await _db.Records
            .Where(r => r.ChildProfileId == childId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(limit)
            .Select(r => new ProgressAttemptDto
            {
                Id = r.Id,
                ExerciseId = r.ExerciseId,
                ExerciseName = $"Exercise #{r.ExerciseId}",
                Accuracy = r.Accuracy,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(attempts);
    }

    [HttpGet("child/{childId}/trend")]
    public async Task<ActionResult<List<TrendPointDto>>> GetTrend(
        int childId,
        [FromQuery] int days = 14)
    {
        var from = DateTime.UtcNow.Date.AddDays(-days);

        var data = await _db.Records
            .Where(r =>
                r.ChildProfileId == childId &&
                r.CreatedAt >= from)
            .GroupBy(r => r.CreatedAt.Date)
            .Select(g => new TrendPointDto
            {
                Date = g.Key,
                Value = Math.Round(g.Average(x => x.Accuracy), 2)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();

        return Ok(data);
    }
}
