namespace UserService.Contracts;

public class AssignLogopedDto
{
    public string LogopedEmail { get; set; } = "";
}

public class LogopedChildDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string? ProblemSounds { get; set; }
}

public class LogopedDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
}