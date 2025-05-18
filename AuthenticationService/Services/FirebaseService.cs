using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace AuthenticationService.Services;

public interface IFirebaseService
{
    Task<string> CreateCustomTokenAsync(string uid);
}

public class FirebaseService : IFirebaseService
{
    private readonly ILogger<FirebaseService> _logger;

    public FirebaseService(ILogger<FirebaseService> logger)
    {
        _logger = logger;
        
        // Initialize Firebase Admin SDK if not already initialized
        if (FirebaseApp.DefaultInstance == null)
        {
            try
            {
                var credential = GoogleCredential.FromFile("attireme-chat-firebase-adminsdk-fbsvc-956c61fe4a.json");
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = credential
                });
                _logger.LogInformation("Firebase Admin SDK initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase Admin SDK");
                throw;
            }
        }
    }

    public async Task<string> CreateCustomTokenAsync(string uid)
    {
        try
        {
            var token = await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(uid);
            _logger.LogInformation("Successfully created Firebase custom token for user {Uid}", uid);
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Firebase custom token for user {Uid}", uid);
            throw;
        }
    }
} 