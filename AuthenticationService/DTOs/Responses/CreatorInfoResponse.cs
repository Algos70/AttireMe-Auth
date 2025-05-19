using System.ComponentModel.DataAnnotations;
using AuthenticationService.Interfaces;

namespace AuthenticationService.DTOs.Responses;

public class CreatorInfoResponse : IGetUserResponse
{
    [Required]
    public string UserId { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public string BusinessName { get; set; }
    
    [Required]
    public string Address { get; set; }
    
    [Required]
    [Phone]
    public string PhoneNumber { get; set; }
} 