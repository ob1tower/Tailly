using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using Tailly.AuthService.Application.Dtos.Requests.AccountDeletion;
using Tailly.AuthService.Application.Dtos.Responses.AccountDeletion;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Service.AccountDeletion;
using Tailly.AuthService.Application.Validators.AccountDeletion;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Infrastructure.Configurations.Extensions;

namespace Tailly.AuthService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountDeletionController : ControllerBase
{
    private readonly IAccountDeletionService _service;
    private readonly RestoreRequestValidator _restoreRequestValidator;
    private readonly DeletionRequestValidator _deletionValidator;

    public AccountDeletionController(IAccountDeletionService service, 
                                     RestoreRequestValidator restoreRequestValidator, 
                                     DeletionRequestValidator deletionValidator)
    {
        _service = service;
        _restoreRequestValidator = restoreRequestValidator;
        _deletionValidator = deletionValidator;
    }

    /// <summary>
    /// Request to delete an account with the possibility of recovery.
    /// </summary>
    /// <param name="request">Request data (user password).</param>
    /// <returns>The date before which the account can be restored.</returns>
    [Authorize(Roles = "Client,Specialist")]
    [EnableRateLimiting("account-deletion")]
    [HttpPost("account/deletion/request")]
    public async Task<IActionResult> RequestDeletion([FromBody] AccountDeletionRequest request)
    {
        var validationResult = await _deletionValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        if (!string.Equals(request.UserId, userId.Value.ToString(), StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        if (roleClaim == null || !Enum.TryParse<RoleType>(roleClaim.Value, true, out var role))
            return BadRequest(AuthErrors.InvalidRole);

        if (role != RoleType.Client && role != RoleType.Specialist)
            return BadRequest(AuthErrors.InvalidRole);

        var result = await _service.RequestDeletionAsync(userId.Value, request.Password, role);

        if (result.IsFailure)
            return BadRequest(result.Error);  

        return Ok(new DeletionResponse
        {
            Ok = true,
            RestoreDeadlineIso = result.Value
        });
    }

    /// <summary>
    /// Get a preview of account recovery using a token.
    /// </summary>
    /// <param name="token">Recovery token.</param>
    [AllowAnonymous]
    [EnableRateLimiting("account-restore")]
    [HttpGet("account/deletion/restore-preview")]
    public async Task<IActionResult> GetRestorePreview([FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(AuthErrors.InvalidVerificationToken);

        var result = await _service.GetRestorePreviewAsync(token);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var (email, roleStr, restoreUntil) = result.Value;

        return Ok(new RestorePreviewResponse
        {
            Email = email,
            RoleLabel = roleStr.Equals("specialist", StringComparison.OrdinalIgnoreCase)
                ? "Специалист"
                : "Клиент",
            RestoreDeadlineIso = restoreUntil
        });
    }

    /// <summary>
    /// Restore an account using a token.
    /// </summary>
    /// <param name="request">Recovery token.</param>
    [AllowAnonymous]
    [EnableRateLimiting("account-restore")]
    [HttpPost("account/deletion/restore")]
    public async Task<IActionResult> Restore([FromBody] RestoreAccountByTokenRequest request)
    {
        var validationResult = await _restoreRequestValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var result = await _service.RestoreAsync(request.Token);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { ok = true });
    }
}