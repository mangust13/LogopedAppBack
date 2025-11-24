namespace ProgressService.Contracts;

public record ProgressCreateDto(
    int UserId,
    int ExerciseId,
    double Accuracy,
    string Feedback,
    string? RecognizedText
);
