namespace UserService.Contracts;

public class CreateChildProfileDto
{
    public string Name { get; set; } = "";
    public DateTime BirthDate { get; set; }
    public string ProblemSounds { get; set; } = "";
}
