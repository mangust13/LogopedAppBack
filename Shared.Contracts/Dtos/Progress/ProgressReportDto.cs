namespace Shared.Contracts.Dtos.Progress;

public class ProgressReportDto
{
    public int ChildProfileId { get; set; }
    public int ExerciseId { get; set; }
    public double Accuracy { get; set; }
    public string Feedback { get; set; } = "";
    public string RecognizedText { get; set; } = "";
}
