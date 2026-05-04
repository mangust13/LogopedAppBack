namespace ProgressService.Contracts;

public class TrackActivityDto
{
    public int ChildId { get; set; }
    public string ActivityType { get; set; } = "";
}

public class StreakDto
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public bool ActiveToday { get; set; }
    public int TotalActiveDays { get; set; }
}