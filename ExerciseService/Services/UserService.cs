using System.Text.Json;

namespace ExerciseService.Services;

public class UserService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : IUserService
{
    public Task<List<ChildDto>> GetLogopedChildren(int logopedId)
    {
        return GetChildren("/logoped/children", "/users/logoped/children");
    }

    public Task<List<ChildDto>> GetMyChildren()
    {
        return GetChildren("/children", "/users/children");
    }

    private async Task<List<ChildDto>> GetChildren(params string[] requestUris)
    {
        var token = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

        foreach (var requestUri in requestUris)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.TryAddWithoutValidation("Authorization", token);

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                continue;

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<ChildDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ChildDto>();
        }

        return new List<ChildDto>();
    }
}