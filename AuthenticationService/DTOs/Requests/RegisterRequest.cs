﻿using System.ComponentModel.DataAnnotations;
using AuthenticationService.Enums;

namespace AuthenticationService.DTOs.Requests;

public class RegisterRequest
{
    [Required]
    [StringLength(50)]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }

    [Required]
    public UserType UserType { get; set; }
}