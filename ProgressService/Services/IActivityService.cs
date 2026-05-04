using ProgressService.Contracts;

namespace ProgressService.Services;

public interface IActivityService
{
    Task TrackAsync(int childId, string activityType);
    Task<StreakDto> GetStreakAsync(int childId);
    Task<List<string>> GetActiveDatesAsync(int childId, int days = 7);
}