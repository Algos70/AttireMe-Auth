using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.DTOs.Requests;

public class AuthenticationRequest
{
    [Required] [EmailAddress] public string Email { get; set; }

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string Password { get; set; }
}