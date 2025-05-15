using System.ComponentModel.DataAnnotations;
using AuthenticationService.Interfaces;

namespace AuthenticationService.DTOs.Requests;

public class UpdateUserRequest : IGetUserResponse
{
    [Required]
    public string FullName { get; set; }
    
    [Required]
    public string Address { get; set; }
    
    [Required]
    [Phone]
    public string PhoneNumber { get; set; }
} 