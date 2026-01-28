using Shared.Contracts.Events.Common;

namespace Shared.Contracts.Events.Exercises;

public class ExerciseAnalysisRequestedEvent : IntegrationEvent
{
    public int ExerciseId { get; init; }
    public int UserId { get; init; }
    public string AudioUri { get; init; } = "";
    public string ReferenceText { get; init; } = "";
}
