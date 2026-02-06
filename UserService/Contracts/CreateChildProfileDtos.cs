namespace UserService.Contracts;

public class CreateChildProfileDto
{
    public string Name { get; set; } = "";
    public DateTime BirthDate { get; set; }
    public string ProblemSounds { get; set; } = "";
}

public class UpdateChildProfileDto
{
    public string Name { get; set; } = "";
    public DateTime BirthDate { get; set; }
    public string? ProblemSounds { get; set; }
}

public class GetChildProfilesDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime BirthDate { get; set; }
    public string? ProblemSounds { get; set; }
    public string? LogopedEmail { get; set; }
}
