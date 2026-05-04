using ProgressService.Contracts;

namespace ProgressService.Services;

public interface IGameProgressService
{
    Task CompleteGameAsync(CompleteGameDto dto);
    Task<SoundRoadmapDto> GetRoadmapAsync(int childId, string sound);
    Task<List<SoundProgressSummaryDto>> GetSoundsSummaryAsync(int childId, IEnumerable<string> sounds);
}