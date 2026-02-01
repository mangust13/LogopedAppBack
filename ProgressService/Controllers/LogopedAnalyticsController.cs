using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgressService.Contracts;
using ProgressService.Infrastructure;

namespace ProgressService.Controllers;

[ApiController]
[Route("logoped")]
[Authorize(Roles = "Logoped")]
public class LogopedAnalyticsController : ControllerBase
{
    private readonly ProgressDbContext _db;

    public LogopedAnalyticsController(ProgressDbContext db)
    {
        _db = db;
    }

    [HttpPost("summary")]
    public async Task<ActionResult<LogopedSummaryDto>> GetSummary([FromBody] int[] childIds)
    {
        var records = await _db.Records
            .Where(r => childIds.Contains(r.ChildProfileId))
            .ToListAsync();

        if (records.Count == 0)
        {
            return Ok(new LogopedSummaryDto
            {
                TotalChildren = childIds.Length,
                TotalAttempts = 0,
                AvgAccuracy = 0,
                LastActivityAt = null
            });
        }

        return Ok(new LogopedSummaryDto
        {
            TotalChildren = childIds.Length,
            TotalAttempts = records.Count,
            AvgAccuracy = Math.Round(records.Average(r => r.Accuracy), 2),
            LastActivityAt = records.Max(r => r.CreatedAt)
        });
    }

    [HttpPost("problem-exercises")]
    public async Task<ActionResult<List<ProblemExerciseDto>>> GetProblemExercises(
        [FromBody] ProblemExercisesRequest req)
    {
        var result = await _db.Records
            .Where(r => req.ChildProfileIds.Contains(r.ChildProfileId))
            .GroupBy(r => r.ExerciseId)
            .Select(g => new ProblemExerciseDto
            {
                ExerciseId = g.Key,
                Attempts = g.Count(),
                AvgAccuracy = Math.Round(g.Average(x => x.Accuracy), 2)
            })
            .Where(x => x.Attempts >= req.MinAttempts)
            .OrderBy(x => x.AvgAccuracy)
            .Take(5)
            .ToListAsync();

        return Ok(result);
    }
}
