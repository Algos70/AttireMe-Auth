using AuthenticationService.DTOs;
using AuthenticationService.Interfaces.Services;
using AuthenticationService.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net.Mail;

namespace AuthenticationService.Services;

public class EmailService : IEmailService
{
    private readonly MailSettings _mailSettings;
    private readonly FrontendSettings _frontendSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<MailSettings> mailSettings,
        IOptions<FrontendSettings> frontendSettings,
        ILogger<EmailService> logger)
    {
        _mailSettings = mailSettings.Value;
        _frontendSettings = frontendSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(Email request)
    {
        if (string.IsNullOrEmpty(request.To))
        {
            _logger.LogError("To is not present.");
            throw new Exception("Invalid email address.");
        }

        try
        {
            _logger.LogInformation($"Email Request - To: {request.To}, Subject: {request.Subject}");
            _logger.LogInformation($"Email Body: {request.Body}");

            var email = new MimeMessage();

            var senderEmail = request.From;
            if (senderEmail == null)
            {
                senderEmail = _mailSettings.EmailFrom;
            }

            _logger.LogInformation($"Email Sender - From: {senderEmail}");
            email.Sender = new MailboxAddress(_mailSettings.DisplayName, senderEmail);
            email.From.Add(new MailboxAddress(_mailSettings.DisplayName, senderEmail));

            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = request.Subject;

            var builder = new BodyBuilder()
            {
                HtmlBody = request.Body,
                TextBody = request.Body
            };
            email.Body = builder.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(_mailSettings.SmtpHost, _mailSettings.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent successfully");
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError($"Null argument error: {ex.ParamName} - {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Email sending failed: {ex.GetType().Name} - {ex.Message}");
            _logger.LogError($"Stack trace: {ex.StackTrace}");
            throw new Exception($"Email sending failed: {ex.Message}");
        }
    }

    public async Task SendConfirmationEmailAsync(string email, string token)
    {
        try
        {
            var confirmationLink = $"{_frontendSettings.FrontendUrl}/confirm-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
            
            var emailMessage = new Email
            {
                To = email,
                Subject = "Email Confirmation",
                Body = $@"<h1>Welcome to AttireMe!</h1>
                       <p>Please confirm your email by clicking the following link:</p>
                       <p><a href='{confirmationLink}'>Confirm Email</a></p>
                       <p>If you didn't request this, please ignore this email.</p>"
            };

            await SendEmailAsync(emailMessage);
            _logger.LogInformation("Confirmation email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending confirmation email to {Email}", email);
            throw;
        }
    }
}