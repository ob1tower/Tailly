using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.BookingService.Application.Dtos.Requests;
using Tailly.BookingService.Application.Errors;
using Tailly.BookingService.Application.Mappers;
using Tailly.BookingService.Application.Service.Interfaces;
using Tailly.BookingService.Core.Common;
using Tailly.BookingService.Core.Models;
using Tailly.BookingService.Infrastructure.Configurations.Extensions;

namespace Tailly.BookingService.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServiceOrderController : ControllerBase
{
    private readonly IServiceOrderService _service;
    private readonly IValidator<CreateServiceOrderRequest> _createValidator;
    private readonly IValidator<LeaveReviewRequest> _reviewValidator;

    public ServiceOrderController(IServiceOrderService service,
                                  IValidator<CreateServiceOrderRequest> createValidator,
                                  IValidator<LeaveReviewRequest> reviewValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _reviewValidator = reviewValidator;
    }

    /// <summary>
    /// Gets list of service orders.
    /// </summary>
    [HttpGet("me/orders/services")]
    [Authorize(Roles = "Client,Specialist")]
    public async Task<IActionResult> GetMyOrders([FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        Result<List<ServiceOrder>, Error> result;

        if (User.IsInRole("Specialist"))
        {
            var specialistId = User.GetSpecialistId();
            if (specialistId == null)
                return Unauthorized();

            result = await _service.GetBySpecialistIdAsync(
                specialistId.Value,
                status,
                page,
                limit);

            if (result.IsFailure)
                return BadRequest(result.Error);

            return Ok(result.Value.Select(ServiceOrderMapper.ToSpecialistResponse));
        }

        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        result = await _service.GetMyOrdersAsync(
            userId.Value,
            status,
            page,
            limit);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value.Select(ServiceOrderMapper.ToClientResponse));
    }

    /// <summary>
    /// Creates a new service order.
    /// </summary>
    [HttpPost("me/orders/services/add")]
    [Authorize(Roles = "Client")]
    [EnableRateLimiting("service-orders")]
    public async Task<IActionResult> Create([FromBody] CreateServiceOrderRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var model = ServiceOrderMapper.ToCreateModel(request);

        model.ClientId = userId.Value;

        var result = await _service.CreateAsync(model);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(ServiceOrderMapper.ToClientResponse(result.Value));
    }

    /// <summary>
    /// Confirms the order.
    /// </summary>
    [HttpPost("me/orders/services/{orderId:guid}/confirm")]
    [Authorize(Roles = "Specialist")]
    [EnableRateLimiting("service-orders")]
    public async Task<IActionResult> Confirm(Guid orderId)
    {
        var specialistId = User.GetSpecialistId();
        if (specialistId == null)
            return Unauthorized();

        var result = await _service.ConfirmAsync(orderId, specialistId.Value);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { ok = true });
    }

    /// <summary>
    /// Starts the order execution.
    /// </summary>
    [HttpPost("me/orders/services/{orderId:guid}/start")]
    [Authorize(Roles = "Specialist")]
    [EnableRateLimiting("service-orders")]
    public async Task<IActionResult> Start(Guid orderId)
    {
        var specialistId = User.GetSpecialistId();
        if (specialistId == null)
            return Unauthorized();

        var result = await _service.StartAsync(orderId, specialistId.Value);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { ok = true });
    }

    /// <summary>
    /// Completes the order.
    /// </summary>
    [HttpPost("me/orders/services/{orderId:guid}/complete")]
    [Authorize(Roles = "Specialist")]
    [EnableRateLimiting("service-orders")]
    public async Task<IActionResult> Complete(Guid orderId)
    {
        var specialistId = User.GetSpecialistId();
        if (specialistId == null)
            return Unauthorized();

        var result = await _service.CompleteAsync(orderId, specialistId.Value);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { ok = true });
    }

    /// <summary>
    /// Cancels the order.
    /// </summary>
    [HttpPost("me/orders/services/{orderId:guid}/cancel")]
    [Authorize(Roles = "Client,Specialist")]
    [EnableRateLimiting("service-orders")]
    public async Task<IActionResult> Cancel(Guid orderId, [FromBody] string? reason = null)
    {
        var userId = User.GetUserId();
        if (userId == null) 
            return Unauthorized();

        var result = await _service.CancelAsync(orderId, userId.Value, reason);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { ok = true });
    }

    /// <summary>
    /// Repeats the order.
    /// </summary>
    [HttpPost("me/orders/services/{orderId:guid}/repeat")]
    [Authorize(Roles = "Client")]
    [EnableRateLimiting("service-orders")]
    public async Task<IActionResult> Repeat(Guid orderId)
    {
        var userId = User.GetUserId();
        if (userId == null) 
            return Unauthorized();

        var result = await _service.RepeatAsync(orderId, userId.Value);

        if (result.IsFailure)
            return BadRequest(result.Error);

        var repeatResult = new
        {
            ok = true,
            draftPayload = result.Value
        };

        return Ok(repeatResult);
    }

    /// <summary>
    /// Leaves a review for completed order.
    /// </summary>
    [HttpPost("me/orders/services/{orderId:guid}/review")]
    [Authorize(Roles = "Client")]
    [EnableRateLimiting("reviews")]
    public async Task<IActionResult> LeaveReview(Guid orderId, [FromBody] LeaveReviewRequest request)
    {
        var validationResult = await _reviewValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validationResult.Errors));

        var userId = User.GetUserId();
        if (userId == null) 
            return Unauthorized();

        var result = await _service.LeaveReviewAsync(orderId, userId.Value, request.Rating, request.Comment, request.Photos);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}