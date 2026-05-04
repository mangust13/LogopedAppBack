namespace ProgressService.Domain;

public class GameProgress
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public string Sound { get; set; } = string.Empty;
    public int PositionCode { get; set; }
    public string GameType { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}