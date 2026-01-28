namespace ExerciseService.Contracts;

public class UploadAudioRequest
{
    public required IFormFile File { get; set; }
    public int ExerciseId { get; set; }
    public int UserId { get; set; }
    public required string ReferenceText { get; set; }
}
