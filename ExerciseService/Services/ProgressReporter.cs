using System.Diagnostics;
using System.Net.Http.Json;
using Shared.Contracts.Events.Exercises;
using Shared.Contracts.Dtos.Progress;

namespace ExerciseService.Services;

public class ProgressReporter
{
    private readonly IHttpClientFactory _clientFactory;

    public ProgressReporter(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task ReportAsync(ExerciseAnalysisCompletedEvent evt)
    {
        var client = _clientFactory.CreateClient("ProgressService");

        var dto = new ProgressReportDto
        {
            ChildProfileId = evt.UserId,
            ExerciseId = evt.ExerciseId,
            Accuracy = evt.Accuracy,
            Feedback = evt.Feedback,
            RecognizedText = evt.RecognizedIpa
        };

        var response = await client.PostAsJsonAsync("/api/progress", dto);

        response.EnsureSuccessStatusCode();
    }
}
