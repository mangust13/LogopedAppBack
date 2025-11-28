namespace ExerciseService.Contracts;

public class UploadAudioRequest
{
    public IFormFile File { get; set; }
    public string ExerciseId { get; set; }
    public string UserId { get; set; }
    public string ReferenceText { get; set; }
}
