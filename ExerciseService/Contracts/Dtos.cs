namespace ExerciseService.Contracts;

public class ExerciseDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string VideoUrl { get; set; }
    public string IconName { get; set; }
    public string Category { get; set; }
}

public class CreateComplexDto
{
    public string Title { get; set; }
    public List<int> ExerciseIds { get; set; }
}

public class ComplexDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<ExerciseDto> Exercises { get; set; }
}

public class AssignHomeworkDto
{
    public int ChildId { get; set; }
    public int ComplexId { get; set; }
}