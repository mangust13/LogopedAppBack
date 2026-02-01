namespace ProgressService.Contracts;

public class ProgressCreateDto
{
    public int ChildProfileId { get; set; }
    public int ExerciseId { get; set; }
    public double Accuracy { get; set; }
    public string Feedback { get; set; } = "";
    public string RecognizedText { get; set; } = "";
}


public class ProgressSummaryDto
{
    public int ChildId { get; set; }
    public int TotalAttempts { get; set; }
    public double AvgAccuracy { get; set; }
    public DateTime? LastActivityAt { get; set; }
}

public class ProgressAttemptDto
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public double? Accuracy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TrendPointDto
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
}

public class LogopedSummaryDto
{
    public int TotalChildren { get; set; }
    public int TotalAttempts { get; set; }
    public double AvgAccuracy { get; set; }
    public DateTime? LastActivityAt { get; set; }
}

public class ProblemExerciseDto
{
    public int ExerciseId { get; set; }
    public int Attempts { get; set; }
    public double AvgAccuracy { get; set; }
}

public class ProblemExercisesRequest
{
    public int[] ChildProfileIds { get; set; } = [];
    public int MinAttempts { get; set; } = 5;
}