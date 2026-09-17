using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.AuthService.Application.Dtos.Requests.Token;
using Tailly.AuthService.Application.Dtos.Responses;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Service.Auth.Token;

namespace Tailly.AuthService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SessionController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IValidator<RefreshTokenRequest> _refreshValidator;

    public SessionController(ITokenService tokenService, IValidator<RefreshTokenRequest> refreshValidator)
    {
        _tokenService = tokenService;
        _refreshValidator = refreshValidator;
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// </summary>
    /// <param name="request">Refresh token.</param>
    /// <returns>New access and refresh tokens.</returns>
    [HttpPost("refresh")]
    [EnableRateLimiting("token")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var validationResult = await _refreshValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var result = await _tokenService.RefreshTokenAsync(request.RefreshToken);

        if (result.IsFailure)
            return Unauthorized(result.Error);

        var response = new AuthResponse
        {
            AccessToken = result.Value.AccessToken,
            RefreshToken = result.Value.RefreshToken,
            AccessTokenExpires = result.Value.AccessTokenExpires,
            RefreshTokenExpires = result.Value.RefreshTokenExpires
        };

        return Ok(response);
    }

    /// <summary>
    /// Logs out the user by invalidating the refresh token.
    /// </summary>
    /// <param name="request">Refresh token to be invalidated.</param>
    [HttpPost("logout")]
    [EnableRateLimiting("token")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var validationResult = await _refreshValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var result = await _tokenService.LogoutAsync(request.RefreshToken);

        if (result.IsFailure)
            return Unauthorized(result.Error);

        return Ok();
    }
}