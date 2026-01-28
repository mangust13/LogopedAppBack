using Shared.Contracts.Events.Common;

namespace Shared.Contracts.Events.Exercises;

public class ExerciseAnalysisCompletedEvent : IntegrationEvent
{
    public int ExerciseId { get; init; }
    public int UserId { get; init; }
    public double Accuracy { get; init; }
    public string Feedback { get; init; } = "";
    public string RecognizedIpa { get; init; } = "";
}
