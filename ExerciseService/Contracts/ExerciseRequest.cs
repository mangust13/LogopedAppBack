namespace ExerciseService.Contracts;

public class ExerciseRequest
{
    public int ExerciseId { get; set; }
    public int UserId { get; set; }
    public string AudioUrl { get; set; } = "";
    public string ReferenceText { get; set; } = "";
}
