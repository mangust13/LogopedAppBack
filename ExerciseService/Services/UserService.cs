// Services/UserService.cs в ExerciseService
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ExerciseService.Services;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(HttpClient httpClient, ILogger<UserService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<ChildDto>> GetLogopedChildren(int logopedId)
    {
        try
        {
            _logger.LogInformation("Getting children for logoped ID: {LogopedId}", logopedId);

            // Отримуємо токен з поточного запиту
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(token))
            {
                // Додаємо токен до запиту
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", token);
                _logger.LogInformation("Added Authorization header: {Token}", token);
            }
            else
            {
                _logger.LogWarning("No Authorization token found in the current request");
            }

            // Використовуємо повний URL
            var requestUri = "/api/users/logoped/children";
            _logger.LogInformation("Request URI: {RequestUri}", requestUri);

            var response = await _httpClient.GetAsync(requestUri);

            _logger.LogInformation("Response status code: {StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Response content: {Content}", json);

                var children = JsonSerializer.Deserialize<List<ChildDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new();

                _logger.LogInformation("Deserialized {Count} children", children.Count);
                foreach (var child in children)
                {
                    _logger.LogInformation("Child ID: {ChildId}, Name: {Name}", child.Id, child.Name);
                }

                return children;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error getting children. Status code: { StatusCode}, Content: { ErrorContent}", 
                response.StatusCode, errorContent);

            return new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting children for logoped {LogopedId}", logopedId);
            return new();
        }
    }
}