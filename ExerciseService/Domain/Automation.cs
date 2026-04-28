namespace ExerciseService.Domain;

public class SoundCard
{
    public int Id { get; set; }
    public string Sound { get; set; } = string.Empty;
    public string Word { get; set; } = string.Empty;
    public string ImageFile { get; set; } = string.Empty;

    public int PositionId { get; set; }
    public SoundPosition Position { get; set; } = null!;
}

public class SoundPosition
{
    public int Id { get; set; }
    public int Code { get; set; }
    public string DisplayName { get; set; } = string.Empty;

    public List<SoundCard> SoundCards { get; set; } = new();
}