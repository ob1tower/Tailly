using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.AuthService.Application.Dtos.Requests.Login;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Mappers;
using Tailly.AuthService.Application.Service.Auth.Login;

namespace Tailly.AuthService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly ILoginService _loginService;
    private readonly IValidator<LoginRequest> _loginValidator;

    public LoginController(ILoginService loginService, IValidator<LoginRequest> loginValidator)
    {
        _loginService = loginService;
        _loginValidator = loginValidator;
    }

    /// <summary>
    /// Authenticates a user and returns JWT tokens.
    /// </summary>
    /// <param name="request">Login credentials and requested role.</param>
    /// <returns>Authentication result containing access token and user data.</returns>
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var validationResult = await _loginValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        if (!AuthMapper.TryParseRole(request.RequestedRole, out var role))
            return BadRequest(AuthErrors.InvalidRole);

        var result = await _loginService.LoginAsync(request.Email, request.Password, role);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Auth.InvalidCredentials" => Unauthorized(result.Error),
                "Auth.AccountBlocked" => Unauthorized(result.Error),
                "Auth.EmailNotConfirmed" => Unauthorized(result.Error),
                "Auth.InvalidRole" => BadRequest(result.Error),
                _ => BadRequest(result.Error)
            };
        }

        return Ok(result.Value);
    }
}