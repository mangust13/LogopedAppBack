namespace UserService.Domain;

public class ChildProfile
{
    public int Id { get; set; }

    public int ParentUserId { get; set; }
    public User ParentUser { get; set; } = null!;

    public string Name { get; set; } = "";
    public DateTime BirthDate { get; set; }

    public string ProblemSounds { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
