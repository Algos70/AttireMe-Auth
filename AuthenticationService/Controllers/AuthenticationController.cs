using AuthenticationService.DTOs.Requests;
using AuthenticationService.DTOs.Responses;
using AuthenticationService.Enums;
using AuthenticationService.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using RegisterRequest = AuthenticationService.DTOs.Requests.RegisterRequest;

namespace AuthenticationService.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController(IAccountService accountService, ITokenService tokenService) : ControllerBase
{
    [HttpPost("/register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var status = await accountService.RegisterAsync(registerRequest);
        return status switch
        {
            RegistrationOutcomes.EmailAlreadyExists => StatusCode(StatusCodes.Status409Conflict,
                new ProblemDetails { Detail = "Email already exists." }),
            RegistrationOutcomes.SystemError => StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails { Detail = "User generation failed possibly due to invalid password." }),
            RegistrationOutcomes.EmailCantBeSend => StatusCode(StatusCodes.Status400BadRequest,
                new ProblemDetails { Detail = "Email cant be sent possibly due to invalid email." }),
            RegistrationOutcomes.Success => Ok(),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails { Detail = "Unexpected error." })
        };
    }

    [HttpPost("/authenticate")]
    public async Task<IActionResult> Login([FromBody] AuthenticationRequest authenticationRequest)
    {
        var (status, response) = await accountService.AuthenticateAsync(authenticationRequest);
        return status switch
        {
            AuthenticationOutcomes.EmailNotFound => StatusCode(StatusCodes.Status404NotFound,
                new ProblemDetails { Detail = "Email not found" }),
            AuthenticationOutcomes.WrongPassword => StatusCode(StatusCodes.Status401Unauthorized,
                new ProblemDetails { Detail = "Wrong password" }),
            AuthenticationOutcomes.Success => Ok(response),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails { Detail = "Unexpected error." })
        };
    }

    [HttpPost("/confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest confirmEmailRequest)
    {
        var status = await accountService.ConfirmEmailAsync(confirmEmailRequest);
        return status switch
        {
            ConfirmEmailOutcomes.EmailNotFound => StatusCode(StatusCodes.Status404NotFound,
                new ProblemDetails { Detail = "Email not found." }),
            ConfirmEmailOutcomes.InvalidToken => StatusCode(StatusCodes.Status401Unauthorized,
                new ProblemDetails { Detail = "Invalid reset token" }),
            ConfirmEmailOutcomes.Success => Ok(),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails { Detail = "Unexpected error." })
        };
    }

    [HttpPost("/request-password-reset")]
    public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequest passwordResetRequest)
    {
        var status = await accountService.RequestPasswordReset(passwordResetRequest);
        return status switch
        {
            RequestPasswordResetOutcomes.EmailNotFound => StatusCode(StatusCodes.Status404NotFound,
                new ProblemDetails { Detail = "Email not found." }),
            RequestPasswordResetOutcomes.EmailCantBeSend => StatusCode(StatusCodes.Status400BadRequest,
                new ProblemDetails { Detail = "Email cant be sent possibly due to invalid email." }),
            RequestPasswordResetOutcomes.Success => Ok(),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails { Detail = "Unexpected error." })
        };
    }

    [HttpPost("/confirm-password-reset")]
    public async Task<IActionResult> ConfirmPasswordReset(
        [FromBody] ConfirmPasswordResetRequest confirmPasswordResetRequest)
    {
        var status = await accountService.ConfirmPasswordReset(confirmPasswordResetRequest);
        return status switch
        {
            ConfirmPasswordResetOutcomes.EmailNotFound => StatusCode(StatusCodes.Status404NotFound,
                new ProblemDetails { Detail = "Email not found." }),
            ConfirmPasswordResetOutcomes.UnsupportedPasswordFormat => StatusCode(StatusCodes.Status400BadRequest,
                new ProblemDetails
                    { Detail = "Password could not be reset due to invalid password format or invalid token." }),
            ConfirmPasswordResetOutcomes.Success => Ok(),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails { Detail = "Unexpected error." })
        };
    }

    [HttpPost("/refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
    {
        var (status, response) = await tokenService.RefreshTokenAsync(refreshTokenRequest);
        return status switch
        {
            RefreshTokenOutcomes.EmailNotFound => StatusCode(StatusCodes.Status404NotFound,
                new ProblemDetails { Detail = "Email not found. This token belongs to no user." }),
            RefreshTokenOutcomes.Expired => StatusCode(StatusCodes.Status401Unauthorized,
                new ProblemDetails { Detail = "Token is expired." }),
            RefreshTokenOutcomes.Success => Ok(response),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails { Detail = "Unexpected error." })
        };
    }

    [HttpPost("/user-policy")]
    public IActionResult CheckForUserPolicy([FromBody] CheckForPolicyRequest request)
    {
        var status = accountService.CheckForUserPolicy(request);
        return status switch
        {
            CheckForPolicyOutcomes.Failure => StatusCode(StatusCodes.Status401Unauthorized, 
                new ProblemDetails { Detail = "Does not belong to user policy."}),
            CheckForPolicyOutcomes.Success => Ok(),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails { Detail = "Unexpected error." })
        };
    }
    
    [HttpPost("/creator-policy")]
    public IActionResult CheckForCreatorPolicy([FromBody] CheckForPolicyRequest request)
    {
        var status = accountService.CheckForCreatorPolicy(request);
        return status switch
        {
            CheckForPolicyOutcomes.Failure => StatusCode(StatusCodes.Status401Unauthorized, 
                new ProblemDetails { Detail = "Does not belong to creator policy."}),
            CheckForPolicyOutcomes.Success => Ok(),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails { Detail = "Unexpected error." })
        };
    }
    
    [HttpPost("/admin-policy")]
    public IActionResult CheckForAdminPolicy([FromBody] CheckForPolicyRequest request)
    {
        var status = accountService.CheckForAdminPolicy(request);
        return status switch
        {
            CheckForPolicyOutcomes.Failure => StatusCode(StatusCodes.Status401Unauthorized, 
                new ProblemDetails { Detail = "Does not belong to admin policy."}),
            CheckForPolicyOutcomes.Success => Ok(),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails { Detail = "Unexpected error." })
        };
    }

    [HttpGet("/user/{email}")]
    public async Task<IActionResult> GetUserInfo([FromRoute] string email)
    {
        var (status, response) = await accountService.GetUserInfo(email);
        return status switch
        {
            GetUserInfoOutcomes.EmailNotFound => StatusCode(StatusCodes.Status404NotFound,
                new ProblemDetails { Detail = "Email not found. There is no such user" }),
            GetUserInfoOutcomes.UserIsAdmin => StatusCode(StatusCodes.Status404NotFound,
                new ProblemDetails { Detail = "Admin users don't have user details." }),
            GetUserInfoOutcomes.UserNotInitialized => StatusCode(StatusCodes.Status404NotFound, new ProblemDetails
                { Detail = "User is not initialized due to unknown reason." }),
            GetUserInfoOutcomes.CreatorNotInitialized => StatusCode(StatusCodes.Status404NotFound, new ProblemDetails
                { Detail = "Creator is not initialized due to unknown reason." }),
            GetUserInfoOutcomes.Success => Ok(response),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails { Detail = "Unexpected error." })
        };
    }

    [HttpPut("/user/{email}")]
    public async Task<IActionResult> UpdateUserInfo([FromRoute] string email, [FromBody] UpdateUserRequest request)
    {
        var status = await accountService.UpdateUserInfo(email, request, "User");
        return status switch
        {
            UpdateUserInfoOutcomes.EmailNotFound => StatusCode(StatusCodes.Status404NotFound,
                new ProblemDetails { Detail = "Email not found. There is no such user" }),
            UpdateUserInfoOutcomes.UserIsAdmin => StatusCode(StatusCodes.Status404NotFound,
                new ProblemDetails { Detail = "Admin users don't have user details." }),
            UpdateUserInfoOutcomes.InvalidToken => StatusCode(StatusCodes.Status401Unauthorized,
                new ProblemDetails { Detail = "Invalid json web token" }),
            UpdateUserInfoOutcomes.Success => Ok(),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails { Detail = "Unexpected error." })
        };
    }
    
    [HttpPut("/creator/{email}")]
    public async Task<IActionResult> UpdateCreatorInfo([FromRoute] string email, [FromBody] UpdateCreatorRequest request)
    {
        var status = await accountService.UpdateCreatorInfo(email, request, "Creator");
        return status switch
        {
            UpdateUserInfoOutcomes.EmailNotFound => StatusCode(StatusCodes.Status404NotFound,
                new ProblemDetails { Detail = "Email not found. There is no such user" }),
            UpdateUserInfoOutcomes.UserIsAdmin => StatusCode(StatusCodes.Status404NotFound,
                new ProblemDetails { Detail = "Admin users don't have user details." }),
            UpdateUserInfoOutcomes.WrongUserType => StatusCode(StatusCodes.Status403Forbidden,
                new ProblemDetails { Detail = "A user can't be updated with creator info and vice versa"}),
            UpdateUserInfoOutcomes.InvalidToken => StatusCode(StatusCodes.Status401Unauthorized,
                new ProblemDetails { Detail = "Invalid json web token" }),
            UpdateUserInfoOutcomes.Success => Ok(),
            _ => StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails { Detail = "Unexpected error." })
        };
    }
}