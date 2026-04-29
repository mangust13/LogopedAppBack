namespace ExerciseService.Domain;

public class Complex
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;
    public int? LogopedId { get; set; }
    public bool IsDefault { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public List<ComplexItem> Exercises { get; set; } = new();
    public List<ComplexAssignment> Assignments { get; set; } = new();
}

public class ComplexItem
{
    public int Id { get; set; }

    public int ComplexId { get; set; }
    public Complex Complex { get; set; } = null!;

    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public int Order { get; set; }
}

public class ComplexAssignment
{
    public int Id { get; set; }

    public int ComplexId { get; set; }
    public Complex Complex { get; set; } = null!;

    public int ChildId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public bool IsActive { get; set; } = true;
}