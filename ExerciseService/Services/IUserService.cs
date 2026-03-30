// Services/IUserService.cs
namespace ExerciseService.Services;

public interface IUserService
{
    Task<List<ChildDto>> GetLogopedChildren(int logopedId);
}

public class ChildDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string? ProblemSounds { get; set; }
}