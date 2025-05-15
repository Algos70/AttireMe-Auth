using System.ComponentModel.DataAnnotations;
using AuthenticationService.Interfaces;

namespace AuthenticationService.DTOs.Requests;

public class UpdateCreatorRequest : IGetUserResponse
{
    [Required]
    public string BusinessName { get; set; }
    
    [Required]
    public string Address { get; set; }
    
    [Required]
    [Phone]
    public string PhoneNumber { get; set; }
} 