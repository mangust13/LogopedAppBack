namespace ExerciseService.Services;

public interface IUserService
{
    Task<List<ChildDto>> GetLogopedChildren(int logopedId);
    Task<List<ChildDto>> GetMyChildren();
}

public class ChildDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime BirthDate { get; set; }
    public string? ProblemSounds { get; set; }
    public string? LogopedEmail { get; set; }
}