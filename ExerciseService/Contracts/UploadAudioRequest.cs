namespace ExerciseService.Contracts;

public class UploadAudioRequest
{
    public IFormFile File { get; set; }
    public int ExerciseId { get; set; }
    public int UserId { get; set; }
    public string ReferenceText { get; set; }
}
