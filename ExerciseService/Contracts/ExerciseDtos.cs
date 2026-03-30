//Contracts/ExerciseDtos.cs
namespace ExerciseService.Contracts;

public class ExerciseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VideoPath { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;

    public List<ExerciseTagDto> Tags { get; set; } = new();
}

public class ExerciseTagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class ExerciseMainCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;
    public int ExerciseCount { get; set; }
}
