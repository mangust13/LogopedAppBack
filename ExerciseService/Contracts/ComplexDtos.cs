// Contracts/ComplexDtos.cs
namespace ExerciseService.Contracts;

public class CreateComplexRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<int> ExerciseIds { get; set; } = new();
}

public class ComplexDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? LogopedId { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public int ExerciseCount { get; set; }
    public List<ExerciseDto> Exercises { get; set; } = new();
}

public class AssignComplexRequest
{
    public int ComplexId { get; set; }
    public List<int> ChildIds { get; set; } = new();
}

public class ComplexAssignmentDto
{
    public int Id { get; set; }
    public int ComplexId { get; set; }
    public string ComplexName { get; set; } = string.Empty;
    public int ChildId { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsActive { get; set; }
}