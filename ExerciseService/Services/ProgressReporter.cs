using System.Diagnostics;
using System.Net.Http.Json;

namespace ExerciseService.Services;

public class ProgressReporter
{
    private readonly IHttpClientFactory _clientFactory;
    private static readonly ActivitySource Activity = new ActivitySource("ExerciseService.RabbitMQ");

    public ProgressReporter(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task ReportAsync(string userId, string exerciseId, double accuracy, string feedback, string ipa)
    {
        using var activity = Activity.StartActivity("Report to ProgressService");

        activity?.SetTag("exercise.id", exerciseId);
        activity?.SetTag("user.id", userId);

        var client = _clientFactory.CreateClient("ProgressService");

        var payload = new
        {
            UserId = int.Parse(userId),
            ExerciseId = int.Parse(exerciseId),
            Accuracy = accuracy,
            Feedback = feedback,
            RecognizedText = ipa
        };

        await client.PostAsJsonAsync("/api/progress", payload);
        activity?.SetTag("status", "sent");
    }
}
