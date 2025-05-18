// StreamChatService.cs
using System;
using System.Threading.Tasks;
using AuthenticationService.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StreamChat.Clients;

namespace AuthenticationService.Services
{
    public class StreamChatService : IStreamChatService
    {
        private readonly IUserClient _userClient;
        private readonly ILogger<StreamChatService> _logger;

        public StreamChatService(
            IConfiguration configuration,
            ILogger<StreamChatService> logger)
        {
            var apiKey    = configuration["StreamChat:ApiKey"];
            var apiSecret = configuration["StreamChat:ApiSecret"];
            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret))
                throw new InvalidOperationException("Stream Chat credentials not configured.");

            var factory     = new StreamClientFactory(apiKey, apiSecret);
            _userClient     = factory.GetUserClient();    // Creates IUserClient :contentReference[oaicite:5]{index=5}
            _logger         = logger;
        }

        public Task<string> GenerateTokenAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Generating Stream Chat token for user {UserId}", userId);
                var token = _userClient.CreateToken(userId);  // Synchronous API :contentReference[oaicite:6]{index=6}
                return Task.FromResult(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate Stream Chat token for user {UserId}", userId);
                throw;
            }
        }
    }
}
