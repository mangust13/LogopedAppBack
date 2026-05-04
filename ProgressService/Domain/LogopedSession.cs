namespace ProgressService.Domain;

public class LogopedSession
{
    public int Id { get; set; }
    public int LogopedId { get; set; }
    public int ChildId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? Notes { get; set; }
    public string? SoundsWorkedOn { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}