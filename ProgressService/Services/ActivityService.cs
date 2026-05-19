using Microsoft.EntityFrameworkCore;
using ProgressService.Contracts;
using ProgressService.Domain;
using ProgressService.Infrastructure;

namespace ProgressService.Services;

public class ActivityService : IActivityService
{
    private readonly ProgressDbContext _db;

    public ActivityService(ProgressDbContext db)
    {
        _db = db;
    }

    public async Task TrackAsync(int childId, string activityType)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var exists = await _db.DailyActivities
            .AnyAsync(a => a.ChildId == childId && a.Date == today && a.ActivityType == activityType);

        if (exists) return;

        _db.DailyActivities.Add(new DailyActivity
        {
            ChildId = childId,
            Date = today,
            ActivityType = activityType,
        });

        await _db.SaveChangesAsync();
    }

    public async Task<StreakDto> GetStreakAsync(int childId)
    {
        var dates = await _db.DailyActivities
            .Where(a => a.ChildId == childId)
            .Select(a => a.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToListAsync();

        if (dates.Count == 0)
            return new StreakDto();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeToday = dates.Contains(today);

        var currentStreak = 0;
        var check = activeToday ? today : today.AddDays(-1);

        foreach (var date in dates)
        {
            if (date == check)
            {
                currentStreak++;
                check = check.AddDays(-1);
            }
            else if (date < check)
            {
                break;
            }
        }

        var longestStreak = 0;
        var current = 1;

        for (var i = 1; i < dates.Count; i++)
        {
            if (dates[i] == dates[i - 1].AddDays(-1))
            {
                current++;
            }
            else
            {
                longestStreak = Math.Max(longestStreak, current);
                current = 1;
            }
        }

        longestStreak = Math.Max(longestStreak, current);

        return new StreakDto
        {
            CurrentStreak = currentStreak,
            LongestStreak = longestStreak,
            ActiveToday = activeToday,
            TotalActiveDays = dates.Count,
        };
    }

    public async Task<List<string>> GetActiveDatesAsync(int childId, int days = 7)
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-(days - 1)));

        var dates = await _db.DailyActivities
            .Where(a => a.ChildId == childId && a.Date >= from)
            .Select(a => a.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();

        return dates.Select(d => d.ToString("yyyy-MM-dd")).ToList();
    }

    public async Task<List<InactiveChildDto>> GetInactiveChildrenAsync(
        IEnumerable<int> childIds, int thresholdDays)
    {
        var ids = childIds.ToList();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var threshold = today.AddDays(-thresholdDays);

        var lastActivities = await _db.DailyActivities
            .Where(a => ids.Contains(a.ChildId))
            .GroupBy(a => a.ChildId)
            .Select(g => new
            {
                ChildId = g.Key,
                LastDate = g.Max(a => a.Date)
            })
            .ToListAsync();

        var result = new List<InactiveChildDto>();

        foreach (var childId in ids)
        {
            var last = lastActivities.FirstOrDefault(x => x.ChildId == childId);

            if (last == null)
            {
                result.Add(new InactiveChildDto
                {
                    ChildId = childId,
                    DaysInactive = thresholdDays + 1,
                });
            }
            else if (last.LastDate <= threshold)
            {
                result.Add(new InactiveChildDto
                {
                    ChildId = childId,
                    DaysInactive = today.DayNumber - last.LastDate.DayNumber,
                });
            }
        }

        return result.OrderByDescending(x => x.DaysInactive).ToList();
    }
}