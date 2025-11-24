namespace ExerciseService.Domain;

public class Exercise
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string ReferenceText { get; set; } = "";
    public string AudioExampleUrl { get; set; } = "";
}
