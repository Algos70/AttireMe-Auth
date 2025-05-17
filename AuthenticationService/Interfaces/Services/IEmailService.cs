using AuthenticationService.DTOs;

namespace AuthenticationService.Interfaces.Services;

public interface IEmailService
{
    Task SendEmailAsync(Email request);
    Task SendConfirmationEmailAsync(string email, string token);
}