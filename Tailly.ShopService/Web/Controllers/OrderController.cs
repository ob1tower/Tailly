using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tailly.ShopService.Application.Dtos.Requests.Order;
using Tailly.ShopService.Application.Errors;
using Tailly.ShopService.Application.Mappers;
using Tailly.ShopService.Application.Service.Interfaces;
using Tailly.ShopService.Core.Models.Order.Checkout;
using Tailly.ShopService.Infrastructure.Configurations.Extensions;

namespace Tailly.ShopService.Web.Controllers;

[Authorize(Roles = "Client,Specialist")]
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    private readonly IValidator<CreateOrderRequest> _createOrderValidator;

    public OrderController(IOrderService orderService,
                           ICartService cartService,
                           IValidator<CreateOrderRequest> сreateOrderValidator)
    {
        _orderService = orderService;
        _cartService = cartService;
        _createOrderValidator = сreateOrderValidator;
    }

    /// <summary>
    /// Creates a new order from the current user's cart (checkout process).
    /// </summary>
    /// <param name="request">Checkout form containing recipient details, delivery method, payment method, etc.</param>
    /// <returns>Details of the successfully created order.</returns>
    [HttpPost("orders")]
    [EnableRateLimiting("create-order")]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var validation = await _createOrderValidator.ValidateAsync(request);

        if (!validation.IsValid)
            return BadRequest(ErrorFormatter.Deserialize(validation.Errors));

        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var cartResult = await _cartService.GetCartAsync(userId, null);

        if (cartResult.IsFailure)
            return BadRequest(cartResult.Error);

        var cart = cartResult.Value;

        if (!cart.Items.Any())
            return BadRequest(ShopErrors.CartIsEmpty);

        var coreForm = new CheckoutForm
        {
            Recipient = new CheckoutRecipient
            {
                FirstName = request.Form.Recipient.FirstName,
                LastName = request.Form.Recipient.LastName,
                Phone = request.Form.Recipient.Phone,
                Email = request.Form.Recipient.Email
            },

            DeliveryMethod = ShopMapper.ParseDeliveryMethod(request.Form.DeliveryMethod),
            PaymentMethod = ShopMapper.ParsePaymentMethod(request.Form.PaymentMethod),

            PickupPointId = request.Form.PickupPointId,

            Address = request.Form.Address != null ? new CheckoutAddress
            {
                City = request.Form.Address.City,
                Street = request.Form.Address.Street,
                House = request.Form.Address.House,
                Apartment = request.Form.Address.Apartment,
                Comment = request.Form.Address.Comment
            } : null
        };

        var result = await _orderService.CreateOrderAsync(userId.Value, coreForm, cart.Items);

        if (result.IsFailure)
            return BadRequest(result.Error);

        await _cartService.ClearCartAsync(userId, null);

        return Ok(OrderResponseMapper.ToResponse(result.Value));
    }

    /// <summary>
    /// Gets all orders belonging to the currently authenticated user.
    /// </summary>
    /// <returns>List of the user's orders.</returns>
    [HttpGet("me/orders/products")]
    public async Task<IActionResult> GetUserOrders()
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _orderService.GetUserOrdersAsync(userId.Value);

        return Ok(result.Value.Select(OrderResponseMapper.ToResponse));
    }

    /// <summary>
    /// Gets detailed information about a specific order by its ID.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <returns>Full order details if the user has access to it.</returns>
    [HttpGet("me/orders/products/{orderId:guid}")]
    [EnableRateLimiting("product")]
    public async Task<IActionResult> GetById(Guid orderId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _orderService.GetOrderByIdAsync(userId.Value, orderId);

        if (result.IsFailure)
        {
            return result.Error.Equals(ShopErrors.OrderNotFound)
                ? NotFound(result.Error)
                : BadRequest(result.Error);
        }

        return Ok(OrderResponseMapper.ToResponse(result.Value));
    }

    /// <summary>
    /// Creates a repeat checkout draft based on a previous order.
    /// Useful for re-ordering the same items.
    /// </summary>
    /// <param name="orderId">ID of the order to repeat.</param>
    /// <returns>Repeat draft containing items ready for new checkout.</returns>
    [HttpPost("me/orders/products/{orderId:guid}/repeat")]
    [EnableRateLimiting("order-actions")]
    public async Task<IActionResult> Repeat(Guid orderId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _orderService.RepeatOrderAsync(userId.Value, orderId);

        if (result.IsFailure)
        {
            return result.Error.Equals(ShopErrors.OrderNotFound)
                ? NotFound(result.Error)
                : BadRequest(result.Error);
        }

        return Ok(OrderResponseMapper.ToRepeatDraftResponse(result.Value));
    }

    /// <summary>
    /// Cancels a specific order if it is still eligible for cancellation.
    /// </summary>
    /// <param name="orderId">ID of the order to cancel.</param>
    /// <returns>Success response if cancellation was successful.</returns>
    [HttpPost("me/orders/products/{orderId:guid}/cancel")]
    [EnableRateLimiting("order-actions")]
    public async Task<IActionResult> Cancel(Guid orderId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _orderService.CancelOrderAsync(userId.Value, orderId);

        if (result.IsFailure)
        {
            return result.Error.Equals(ShopErrors.OrderNotFound)
                ? NotFound(result.Error)
                : BadRequest(result.Error);
        }

        return Ok(new { success = true });
    }

    /// <summary>
    /// Starts the payment process for an existing order and returns a payment URL.
    /// </summary>
    /// <param name="orderId">ID of the order to pay for.</param>
    /// <returns>Payment URL for the user to complete the payment.</returns>
    [HttpPost("me/orders/products/{orderId:guid}/pay")]
    [EnableRateLimiting("create-order")]
    public async Task<IActionResult> Pay(Guid orderId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _orderService.StartPaymentAsync(userId.Value, orderId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { paymentUrl = result.Value });
    }

    /// <summary>
    /// Confirms successful payment for an order.
    /// </summary>
    /// <param name="orderId">ID of the order to confirm payment for.</param>
    /// <returns>Success indicator.</returns>
    [HttpPost("me/orders/products/{orderId:guid}/confirm")]
    [EnableRateLimiting("order-actions")]
    public async Task<IActionResult> Confirm(Guid orderId)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized();

        var result = await _orderService.ConfirmPaymentAsync(userId.Value, orderId);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { success = true });
    }
}