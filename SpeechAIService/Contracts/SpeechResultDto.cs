namespace SpeechAIService.Contracts;

public class SpeechResultDto
{
    public string ExerciseId { get; set; } = "";
    public string UserId { get; set; } = "";
    public double AccuracyScore { get; set; }
    public string RecognizedIPA { get; set; } = "";
    public string Feedback { get; set; } = "";
}
