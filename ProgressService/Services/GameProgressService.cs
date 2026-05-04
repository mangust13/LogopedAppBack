using Microsoft.EntityFrameworkCore;
using ProgressService.Contracts;
using ProgressService.Domain;
using ProgressService.Infrastructure;

namespace ProgressService.Services;

public class GameProgressService(ProgressDbContext db, IActivityService activityService) : IGameProgressService
{
    private static readonly string[] GameTypes = ["SwipeGame", "MatchingGame", "ClassificationGame"];

    private static readonly Dictionary<int, string> PositionNames = new()
    {
        { 1, "Початок слова" },
        { 2, "Середина слова" },
        { 3, "Кінець слова" },
        { 4, "Збіг приголосних" },
    };

    private static readonly Dictionary<string, string> GameDisplayNames = new()
    {
        { "SwipeGame", "Гортай картинки" },
        { "MatchingGame", "Знайди однакові" },
        { "ClassificationGame", "Розклади по групах" },
    };

    private const int TotalPositions = 4;
    private const int TotalSteps = TotalPositions * 3;

    public async Task CompleteGameAsync(CompleteGameDto dto)
    {
        var existing = await db.GameProgresses.FirstOrDefaultAsync(p =>
            p.ChildId == dto.ChildId &&
            p.Sound == dto.Sound.ToLower() &&
            p.PositionCode == dto.PositionCode &&
            p.GameType == dto.GameType);

        if (existing is not null)
        {
            existing.IsCompleted = true;
            existing.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            db.GameProgresses.Add(new GameProgress
            {
                ChildId = dto.ChildId,
                Sound = dto.Sound.ToLower(),
                PositionCode = dto.PositionCode,
                GameType = dto.GameType,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow,
            });
        }

        await db.SaveChangesAsync();
        await activityService.TrackAsync(dto.ChildId, "Game");
    }

    public async Task<SoundRoadmapDto> GetRoadmapAsync(int childId, string sound)
    {
        var normalizedSound = sound.ToLower();

        var completedEntries = await db.GameProgresses
            .Where(p => p.ChildId == childId && p.Sound == normalizedSound && p.IsCompleted)
            .Select(p => new { p.PositionCode, p.GameType })
            .ToListAsync();

        var completedSet = completedEntries
            .Select(e => (e.PositionCode, e.GameType))
            .ToHashSet();

        var positions = new List<PositionStatusDto>();

        for (var posCode = 1; posCode <= TotalPositions; posCode++)
        {
            var isUnlocked = posCode == 1 || IsPositionFullyCompleted(completedSet, posCode - 1);

            var games = GameTypes.Select(gameType => new GameStatusDto
            {
                GameType = gameType,
                DisplayName = GameDisplayNames[gameType],
                IsCompleted = completedSet.Contains((posCode, gameType)),
            }).ToList();

            positions.Add(new PositionStatusDto
            {
                PositionCode = posCode,
                DisplayName = PositionNames[posCode],
                IsUnlocked = isUnlocked,
                Games = games,
            });
        }

        var completedSteps = completedSet.Count;

        return new SoundRoadmapDto
        {
            Sound = sound,
            CompletedSteps = completedSteps,
            TotalSteps = TotalSteps,
            ProgressPercent = (int)Math.Round((double)completedSteps / TotalSteps * 100),
            Positions = positions,
        };
    }

    public async Task<List<SoundProgressSummaryDto>> GetSoundsSummaryAsync(int childId, IEnumerable<string> sounds)
    {
        var soundList = sounds.Select(s => s.ToLower()).ToList();

        var completedCounts = await db.GameProgresses
            .Where(p => p.ChildId == childId && p.IsCompleted && soundList.Contains(p.Sound))
            .GroupBy(p => p.Sound)
            .Select(g => new { Sound = g.Key, Count = g.Count() })
            .ToListAsync();

        return soundList.Select(sound =>
        {
            var count = completedCounts.FirstOrDefault(c => c.Sound == sound)?.Count ?? 0;
            return new SoundProgressSummaryDto
            {
                Sound = sound,
                ProgressPercent = (int)Math.Round((double)count / TotalSteps * 100),
            };
        }).ToList();
    }

    private static bool IsPositionFullyCompleted(HashSet<(int PositionCode, string GameType)> completedSet, int posCode)
    {
        return GameTypes.All(gameType => completedSet.Contains((posCode, gameType)));
    }
}