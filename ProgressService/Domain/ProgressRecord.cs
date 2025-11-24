namespace ProgressService.Domain;

public class ProgressRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ExerciseId { get; set; }
    public double Accuracy { get; set; }
    public string Feedback { get; set; } = "";
    public string? RecognizedText { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
