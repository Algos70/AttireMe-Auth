using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.DTOs.Responses;

public class CreatorListResponse
{
    [Required]
    public string UserId { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
} 