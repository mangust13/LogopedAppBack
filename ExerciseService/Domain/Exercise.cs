namespace ExerciseService.Domain;

public class Exercise
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string VideoPath { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public List<ExerciseTagLink> Tags { get; set; } = new();
}

public class ExerciseTag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public List<ExerciseTagLink> Exercises { get; set; } = new();
}

public class ExerciseTagLink
{
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;

    public int TagId { get; set; }
    public ExerciseTag Tag { get; set; } = null!;
}

