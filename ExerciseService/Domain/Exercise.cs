namespace ExerciseService.Domain;

public class Exercise
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string IconName { get; set; } = "happy";
    public string Category { get; set; } = "General";
}

public class ExerciseComplex
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int LogopedId { get; set; }
    public List<ComplexItem> Items { get; set; } = new();
}

public class ComplexItem
{
    public int Id { get; set; }
    public int ComplexId { get; set; }
    public ExerciseComplex Complex { get; set; } = null!;
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public int Order { get; set; }
}

public class ChildHomework
{
    public int Id { get; set; }
    public int ChildProfileId { get; set; }
    public int ComplexId { get; set; }
    public ExerciseComplex Complex { get; set; } = null!;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public bool IsCompleted { get; set; }
}