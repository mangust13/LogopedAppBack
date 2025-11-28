using System.Diagnostics;
using System.Net.Http.Json;

namespace ExerciseService.Services;

public class ProgressReporter
{
    private readonly IHttpClientFactory _clientFactory;

    public ProgressReporter(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task ReportAsync(string userId, string exerciseId, double accuracy, string feedback, string ipa)
    {
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
    }
}
