namespace ProgressService.Contracts;

public class CompleteGameDto
{
    public int ChildId { get; set; }
    public string Sound { get; set; } = string.Empty;
    public int PositionCode { get; set; }
    public string GameType { get; set; } = string.Empty;
}

public class PositionStatusDto
{
    public int PositionCode { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; }
    public List<GameStatusDto> Games { get; set; } = new();
}

public class GameStatusDto
{
    public string GameType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

public class SoundRoadmapDto
{
    public string Sound { get; set; } = string.Empty;
    public int CompletedSteps { get; set; }
    public int TotalSteps { get; set; }
    public int ProgressPercent { get; set; }
    public List<PositionStatusDto> Positions { get; set; } = new();
}

public class SoundProgressSummaryDto
{
    public string Sound { get; set; } = string.Empty;
    public int ProgressPercent { get; set; }
}