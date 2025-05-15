using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Entities;

public class Creator
{
    [Key]
    public string UserId { get; set; }
    public string BusinessName { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public User User { get; set; }
} 