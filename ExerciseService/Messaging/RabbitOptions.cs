namespace ExerciseService.Messaging;

public class RabbitOptions
{
    public string Host { get; set; } = "";
    public string Exchange { get; set; } = "";
    public string AudioRoutingKey { get; set; } = "";
    public string ResultRoutingKey { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
}
