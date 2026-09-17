using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tailly.AuthService.Application.Dtos.Requests.Admin;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Mappers;
using Tailly.AuthService.Application.Service.AdminProfiles;
using Tailly.AuthService.Application.Validators.Admin;
using Tailly.AuthService.Infrastructure.Configurations.Extensions;

namespace Tailly.AuthService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminProfileController : ControllerBase
{
    private readonly IAdminProfileService _adminProfileService;
    private readonly UpdateAdminProfileRequestValidator _updateValidator;

    public AdminProfileController(IAdminProfileService adminProfileService,
                                  UpdateAdminProfileRequestValidator updateValidator)
    {
        _adminProfileService = adminProfileService;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Get current admin profile.
    /// </summary>
    /// <returns>Admin profile.</returns>
    [HttpGet("admin/profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _adminProfileService.GetAsync(userId.Value);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Update current admin profile.
    /// </summary>
    /// <param name="request">Updated profile data.</param>
    /// <returns>Updated admin profile.</returns>
    [HttpPatch("admin/profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateAdminProfileRequest request)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var result = await _adminProfileService.UpdateAsync(
                    userId.Value,
                    request.FirstName,
                    request.LastName,
                    request.MiddleName,
                    request.BirthDate,
                    request.Phone,
                    AuthMapper.ParseDepartment(request.Department));

        if (result.IsFailure) return BadRequest(result.Error);

        return Ok(result.Value);
    }
}