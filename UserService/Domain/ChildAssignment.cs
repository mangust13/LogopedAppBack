namespace UserService.Domain;

public class ChildAssignment
{
    public int Id { get; set; }

    public int ChildProfileId { get; set; }
    public ChildProfile ChildProfile { get; set; } = null!;

    public int LogopedUserId { get; set; }
    public User Logoped { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
