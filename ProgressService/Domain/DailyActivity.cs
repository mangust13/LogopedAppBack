namespace ProgressService.Domain;

public class DailyActivity
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public DateOnly Date { get; set; }
    public string ActivityType { get; set; } = "";
}