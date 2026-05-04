namespace ProgressService.Contracts;

public class CreateSessionDto
{
    public int ChildId { get; set; }
    public DateTime Date { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Notes { get; set; }
    public string? SoundsWorkedOn { get; set; }
}

public class UpdateSessionDto
{
    public DateTime Date { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Notes { get; set; }
    public string? SoundsWorkedOn { get; set; }
}

public class SessionDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public DateTime Date { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Notes { get; set; }
    public List<string> SoundsWorkedOn { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}