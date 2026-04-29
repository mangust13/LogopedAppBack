//Contracts/ExerciseDtos.cs
using ExerciseService.Domain;
using System.Linq.Expressions;

namespace ExerciseService.Contracts;

public class ExerciseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VideoPath { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;

    public List<ExerciseTagDto> Tags { get; set; } = new();

    public static readonly Expression<Func<Exercise, ExerciseDto>> From = x => new ExerciseDto
    {
        Id = x.Id,
        Title = x.Title,
        Description = x.Description,
        VideoPath = x.VideoPath,
        IconName = x.IconName,
        Tags = x.Tags.Select(t => new ExerciseTagDto
        {
            Id = t.Tag.Id,
            Name = t.Tag.Name,
            Category = t.Tag.Category,
            DisplayName = t.Tag.DisplayName
        }).ToList()
    };
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
