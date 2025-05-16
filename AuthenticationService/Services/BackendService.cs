using AuthenticationService.Interfaces.Services;
using AuthenticationService.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace AuthenticationService.Services;

public class BackendService : IBackendService
{
    private readonly HttpClient _httpClient;
    private readonly BackendSettings _backendSettings;
    private readonly ILogger<BackendService> _logger;

    public BackendService(
        HttpClient httpClient,
        IOptions<BackendSettings> backendSettings,
        ILogger<BackendService> logger)
    {
        _httpClient = httpClient;
        _backendSettings = backendSettings.Value;
        _logger = logger;
    }

    public async Task NotifyUserConfirmedAsync(string email, string role, string username, string adminToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
            
            var requestData = new 
            { 
                email = email,
                role = role.ToLowerInvariant(),
                username = username
            };

            _logger.LogInformation("Sending user confirmation request to backend. Request data: {@RequestData}", requestData);
            
            var response = await _httpClient.PostAsJsonAsync($"{_backendSettings.BackendUrl}/user", requestData);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to notify backend about confirmed user. Status code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while notifying backend about confirmed user");
        }
    }
} 