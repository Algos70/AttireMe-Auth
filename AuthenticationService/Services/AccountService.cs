using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthenticationService.Contexts;
using AuthenticationService.DTOs;
using AuthenticationService.DTOs.Requests;
using AuthenticationService.DTOs.Responses;
using AuthenticationService.Entities;
using AuthenticationService.Enums;
using AuthenticationService.Interfaces;
using AuthenticationService.Interfaces.Services;
using AuthenticationService.Settings;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Extensions;

namespace AuthenticationService.Services;

public class AccountService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ITokenService tokenService,
    UserDbContext dbContext,
    IEmailService emailService,
    IOptions<AppRootSettings> appRootOptions,
    IMapper mapper,
    ILogger<AccountService> logger,
    IBackendService backendService)
    : IAccountService
{
    private readonly AppRootSettings _appRootSettings = appRootOptions.Value;
    private readonly IBackendService _backendService = backendService;
    private readonly ILogger<AccountService> _logger = logger;

    public async Task<RegistrationOutcomes> RegisterAsync(RegisterRequest request)
    {
        var userWithSameEmail = await userManager.FindByEmailAsync(request.Email);
        if (userWithSameEmail != null)
        {
            return RegistrationOutcomes.EmailAlreadyExists;
        }

        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email,
            UserType = request.UserType
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return RegistrationOutcomes.SystemError;
        }

        var userType = request.UserType;
        var role = userType == UserType.User ? Roles.User : Roles.Creator;
        await userManager.AddToRoleAsync(user, role.ToString());

        if (userType == UserType.User)
        {
            var appUser = new AppUser()
            {
                UserId = user.Id,
                FullName = string.Empty,
                Address = string.Empty,
                PhoneNumber = string.Empty
            };
            await dbContext.AppUsers.AddAsync(appUser);
        }
        else
        {
            var creator = new Creator()
            {
                UserId = user.Id,
                BusinessName = string.Empty,
                Address = string.Empty,
                PhoneNumber = string.Empty
            };
            await dbContext.Creators.AddAsync(creator);
        }

        await dbContext.SaveChangesAsync();

        var confirmationToken = await GenerateEmailConfirmationTokenAsync(user);
        try
        {
            await emailService.SendConfirmationEmailAsync(user.Email!, confirmationToken);
        }
        catch (Exception)
        {
            await userManager.DeleteAsync(user);
            await dbContext.SaveChangesAsync();
            return RegistrationOutcomes.EmailCantBeSend;
        }

        return RegistrationOutcomes.Success;
    }

    public async Task<(AuthenticationOutcomes, AuthenticationResponse?)> AuthenticateAsync(
        AuthenticationRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return (AuthenticationOutcomes.EmailNotFound, null);
        }

        var result = await signInManager.PasswordSignInAsync(user, request.Password, false, false);
        if (!result.Succeeded)
        {
            return (AuthenticationOutcomes.WrongPassword, null);
        }

        var jwSecurityToken = await tokenService.GenerateJwToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        user.RefreshTokens.Add(refreshToken);
        await userManager.UpdateAsync(user);
        await dbContext.SaveChangesAsync();
        var response = new AuthenticationResponse()
        {
            RefreshToken = refreshToken.Token,
            JwToken = jwSecurityToken,
        };
        return (AuthenticationOutcomes.Success, response);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
    {
        var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var tokenBytes = Encoding.UTF8.GetBytes(confirmationToken);
        var encodedToken = WebEncoders.Base64UrlEncode(tokenBytes);
        return encodedToken;
    }

    public async Task<ConfirmEmailOutcomes> ConfirmEmailAsync(ConfirmEmailRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return ConfirmEmailOutcomes.EmailNotFound;
        }

        var tokenBytes = WebEncoders.Base64UrlDecode(request.Token);
        var decodedToken = Encoding.UTF8.GetString(tokenBytes);
        var result = await userManager.ConfirmEmailAsync(user, decodedToken);
        
        if (result.Succeeded)
        {
            try
            {
                // Get admin user
                var adminUser = await userManager.FindByEmailAsync("admin@attireme.com");
                if (adminUser == null)
                {
                    // Create admin user if it doesn't exist
                    adminUser = new User
                    {
                        UserName = "admin",
                        Email = "admin@attireme.com",
                        EmailConfirmed = true
                    };
                    var adminResult = await userManager.CreateAsync(adminUser, "Admin123!");
                    if (!adminResult.Succeeded)
                    {
                        _logger.LogError("Failed to create admin user");
                        return ConfirmEmailOutcomes.Success; // Still return success for the original confirmation
                    }
                    await userManager.AddToRoleAsync(adminUser, Roles.Admin.ToString());
                }

                // Get admin token
                var adminToken = await tokenService.GenerateJwToken(adminUser);
                
                // Get user's role and username
                var roles = await userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "User";
                
                // Notify backend
                await _backendService.NotifyUserConfirmedAsync(user.Email!, role, user.UserName!, adminToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify backend about confirmed user");
                // Still return success for the original confirmation
            }
            
            return ConfirmEmailOutcomes.Success;
        }
        
        return ConfirmEmailOutcomes.InvalidToken;
    }

    public async Task<RequestPasswordResetOutcomes> RequestPasswordReset(PasswordResetRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return RequestPasswordResetOutcomes.EmailNotFound;
        }

        var resetCode = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedResetCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetCode));
        
        Debug.Assert(user.Email != null, "user.Email != null");
        var email = new Email()
        {
            Subject = "Reset Your AttireMe Password",
            Body = $@"<h1>Password Reset Request</h1>
                    <p>You have requested to reset your password. Please use the following code to reset your password:</p>
                    <p style='font-size: 24px; font-weight: bold; letter-spacing: 2px; padding: 10px; background-color: #f5f5f5; display: inline-block;'>{encodedResetCode}</p>
                    <p>If you didn't request this, please ignore this email and your password will remain unchanged.</p>
                    <p>This code will expire in 24 hours.</p>",
            To = user.Email
        };

        try
        {
            await emailService.SendEmailAsync(email);
        }
        catch (Exception)
        {
            return RequestPasswordResetOutcomes.EmailCantBeSend;
        }

        return RequestPasswordResetOutcomes.Success;
    }

    public async Task<ConfirmPasswordResetOutcomes> ConfirmPasswordReset(ConfirmPasswordResetRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return ConfirmPasswordResetOutcomes.EmailNotFound;
        }

        try
        {
            var tokenBytes = WebEncoders.Base64UrlDecode(request.Token);
            var decodedResetCode = Encoding.UTF8.GetString(tokenBytes);
            var resetPasswordResult = await userManager.ResetPasswordAsync(user, decodedResetCode, request.Password);
            return resetPasswordResult.Succeeded
                ? ConfirmPasswordResetOutcomes.Success
                : ConfirmPasswordResetOutcomes.UnsupportedPasswordFormat;
        }
        catch (Exception)
        {
            return ConfirmPasswordResetOutcomes.UnsupportedPasswordFormat;
        }
    }

    public IList<string> GetUserRoles(string jwToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwToken);
        var roles = token.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == "role").Select(c => c.Value)
            .ToList();
        return roles;
    }
    
    public string? GetUserEmailFromToken(string jwToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwToken);
        var emailClaim = token.Claims.FirstOrDefault(c => c.Type == "email");
        return emailClaim?.Value;
    }

    public string? GetUsernameFromToken(string jwToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwToken);
        var usernameClaim = token.Claims.FirstOrDefault(c => c.Type == "username");
        return usernameClaim?.Value;
    }

    public CheckForPolicyOutcomes CheckForPolicy(CheckForPolicyRequest request, Roles requiredRole)
    {
        var isValid = tokenService.IsTokenValid(request.Token);
        if (!isValid)
        {
            return CheckForPolicyOutcomes.Failure;
        }
        var email = GetUserEmailFromToken(request.Token);
        if (string.IsNullOrEmpty(email))
        {
            return CheckForPolicyOutcomes.EmailNotConfirmed;
        }
        var user = userManager.FindByEmailAsync(email).Result;
        if ((user == null || !user.EmailConfirmed) && requiredRole != Roles.Admin)
        {
            return CheckForPolicyOutcomes.EmailNotConfirmed;
        }
        var roles = GetUserRoles(request.Token);
        foreach (var role in roles)
        {
            if (role == requiredRole.ToString())
            {
                return CheckForPolicyOutcomes.Success;
            }
        }
        return CheckForPolicyOutcomes.Failure;
    }

    public CheckForPolicyOutcomes CheckForUserPolicy(CheckForPolicyRequest request)
    {
        return CheckForPolicy(request, Roles.User);
    }

    public CheckForPolicyOutcomes CheckForCreatorPolicy(CheckForPolicyRequest request)
    {
        return CheckForPolicy(request, Roles.Creator);
    }

    public CheckForPolicyOutcomes CheckForAdminPolicy(CheckForPolicyRequest request)
    {
        return CheckForPolicy(request, Roles.Admin);
    }

    public async Task<(GetUserInfoOutcomes, IGetUserResponse?)> GetUserInfo(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return (GetUserInfoOutcomes.EmailNotFound, null);
        }
        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        switch (role)
        {
            case nameof(Roles.Admin):
                return (GetUserInfoOutcomes.UserIsAdmin, null);
            case nameof(Roles.User):
                var appUser = await dbContext.AppUsers.FindAsync(user.Id);
                return appUser == null
                    ? (GetUserInfoOutcomes.UserNotInitialized, null)
                    : (GetUserInfoOutcomes.Success, mapper.Map<UpdateUserRequest>(appUser));
            case nameof(Roles.Creator):
                var creator = await dbContext.Creators.FindAsync(user.Id);
                return creator == null
                    ? (GetUserInfoOutcomes.CreatorNotInitialized, null)
                    : (GetUserInfoOutcomes.Success, mapper.Map<UpdateCreatorRequest>(creator));
            default:
                return (GetUserInfoOutcomes.UserNotInitialized, null);
        }
    }

    public async Task<UpdateUserInfoOutcomes> UpdateUserInfo(string email, UpdateUserRequest request, string expectedRole)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return UpdateUserInfoOutcomes.EmailNotFound;
        }
        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        if (role != expectedRole)
        {
            return UpdateUserInfoOutcomes.WrongUserType;
        }
        if (role == nameof(Roles.User))
        {
            var appUser = await dbContext.AppUsers.FindAsync(user.Id);
            if (appUser == null) return UpdateUserInfoOutcomes.EmailNotFound;
            appUser.FullName = request.FullName;
            appUser.Address = request.Address;
            appUser.PhoneNumber = request.PhoneNumber;
            dbContext.AppUsers.Update(appUser);
            await dbContext.SaveChangesAsync();
            return UpdateUserInfoOutcomes.Success;
        }
        return UpdateUserInfoOutcomes.WrongUserType;
    }

    public async Task<UpdateUserInfoOutcomes> UpdateCreatorInfo(string email, UpdateCreatorRequest request, string expectedRole)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return UpdateUserInfoOutcomes.EmailNotFound;
        }
        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        if (role != expectedRole)
        {
            return UpdateUserInfoOutcomes.WrongUserType;
        }
        if (role == nameof(Roles.Creator))
        {
            var creator = await dbContext.Creators.FindAsync(user.Id);
            if (creator == null) return UpdateUserInfoOutcomes.EmailNotFound;
            creator.BusinessName = request.BusinessName;
            creator.Address = request.Address;
            creator.PhoneNumber = request.PhoneNumber;
            dbContext.Creators.Update(creator);
            await dbContext.SaveChangesAsync();
            return UpdateUserInfoOutcomes.Success;
        }
        return UpdateUserInfoOutcomes.WrongUserType;
    }
}