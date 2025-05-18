namespace AuthenticationService.DTOs.Responses;

public class AuthenticationResponse
{
    public string JwToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string FirebaseToken { get; set; } = string.Empty;
}