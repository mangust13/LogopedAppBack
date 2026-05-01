using ExerciseService.Domain;
using System.Linq.Expressions;

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

    public static readonly Expression<Func<Complex, ComplexDto>> FromEntity = complex => new ComplexDto
    {
        Id = complex.Id,
        Name = complex.Name,
        DisplayName = complex.DisplayName,
        Description = complex.Description,
        LogopedId = complex.LogopedId,
        IsDefault = complex.IsDefault,
        CreatedAt = complex.CreatedAt,
        IsActive = complex.IsActive,
        ExerciseCount = complex.Exercises.Count,
        Exercises = complex.Exercises
            .OrderBy(item => item.Order)
            .Select(item => new ExerciseDto
            {
                Id = item.Exercise.Id,
                Title = item.Exercise.Title,
                Description = item.Exercise.Description,
                VideoPath = item.Exercise.VideoPath,
                ImagePath = item.Exercise.ImagePath,
                Tags = item.Exercise.Tags
                    .Select(tagLink => new ExerciseTagDto
                    {
                        Id = tagLink.Tag.Id,
                        Name = tagLink.Tag.Name,
                        Category = tagLink.Tag.Category,
                        DisplayName = tagLink.Tag.DisplayName
                    })
                    .ToList()
            })
            .ToList()
    };
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