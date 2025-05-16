using AuthenticationService.DTOs.Responses;

namespace AuthenticationService.Interfaces.Services;

public interface IBackendService
{
    Task NotifyUserConfirmedAsync(string email, string role, string username, string adminToken);
} 