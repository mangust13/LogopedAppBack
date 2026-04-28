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

public class ChildAssignment
{
    public int Id { get; set; }

    public int ChildProfileId { get; set; }
    public ChildProfile ChildProfile { get; set; } = null!;

    public int LogopedUserId { get; set; }
    public User Logoped { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
