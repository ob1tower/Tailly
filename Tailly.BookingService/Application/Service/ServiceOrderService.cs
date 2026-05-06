using CSharpFunctionalExtensions;
using Tailly.BookingService.Application.Errors;
using Tailly.BookingService.Application.Helpers;
using Tailly.BookingService.Application.Service.Interfaces;
using Tailly.BookingService.Core.Common;
using Tailly.BookingService.Core.Enums;
using Tailly.BookingService.Core.Models;
using Tailly.BookingService.Infrastructure.Repositories;

namespace Tailly.BookingService.Application.Service;

public class ServiceOrderService : IServiceOrderService
{
    private readonly IServiceOrderRepository _orderRepository;
    private readonly ILogger<ServiceOrderService> _logger;

    public ServiceOrderService(IServiceOrderRepository orderRepository, ILogger<ServiceOrderService> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<Result<ServiceOrder, Error>> CreateAsync(CreateServiceOrder model)
    {
        if (model == null)
            return Result.Failure<ServiceOrder, Error>(BookingErrors.InvalidOrder);

        var order = new ServiceOrder
        {
            Id = Guid.NewGuid(),
            Number = OrderNumberGenerator.Generate(),

            ClientId = model.ClientId,
            ClientName = model.ClientName,

            SpecialistId = model.SpecialistId,
            SpecialistName = model.SpecialistName,
            SpecialistSlug = model.SpecialistSlug,

            PetId = model.PetId,
            PetName = model.PetName,

            ServiceId = model.ServiceId,
            ServiceTitle = model.ServiceTitle,
            Price = model.Price,
            PriceUnit = model.PriceUnit,

            StartAt = DateTimeHelper.NormalizeToUtc(model.StartAt),
            EndAt = model.EndAt.HasValue
                ? DateTimeHelper.NormalizeToUtc(model.EndAt.Value)
                : null,

            Comment = model.Comment,

            Status = OrderStatus.PendingConfirmation,
            CreatedAt = DateTime.UtcNow
        };

        await _orderRepository.AddAsync(order);

        _logger.LogInformation("Service order created. OrderId: {OrderId}, ClientId: {ClientId}, SpecialistId: {SpecialistId}",
            order.Id, order.ClientId, order.SpecialistId);

        return Result.Success<ServiceOrder, Error>(order);
    }

    public async Task<Result<ServiceOrder, Error>> GetByIdAsync(Guid orderId, Guid? clientId, Guid? specialistId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
            return Result.Failure<ServiceOrder, Error>(BookingErrors.OrderNotFound);

        if (order.ClientId != clientId && order.SpecialistId != specialistId)
            return Result.Failure<ServiceOrder, Error>(BookingErrors.Forbidden);

        return Result.Success<ServiceOrder, Error>(order);
    }

    public async Task<Result<List<ServiceOrder>, Error>> GetMyOrdersAsync(Guid clientId, string? statusFilter = null, int page = 1, int limit = 20)
    {
        var orders = await _orderRepository.GetByClientIdAsync(clientId, statusFilter, page, limit);

        _logger.LogInformation("Retrieved {Count} orders for client {ClientId}", orders.Count, clientId);

        return Result.Success<List<ServiceOrder>, Error>(orders);
    }

    public async Task<Result<List<ServiceOrder>, Error>> GetBySpecialistIdAsync(Guid specialistId, string? statusFilter = null, int page = 1, int limit = 20)
    {
        var orders = await _orderRepository.GetBySpecialistIdAsync(specialistId, statusFilter, page, limit);

        _logger.LogInformation("Retrieved {Count} orders for specialist {SpecialistId}", orders.Count, specialistId);

        return Result.Success<List<ServiceOrder>, Error>(orders);
    }

    public async Task<Result> ConfirmAsync(Guid orderId, Guid specialistId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return Result.Failure(BookingErrors.OrderNotFound.Description);

        if (order.SpecialistId != specialistId)
            return Result.Failure(BookingErrors.Forbidden.Description);

        order.Status = OrderStatus.Confirmed;
        order.ConfirmedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);
        return Result.Success();
    }

    public async Task<Result> StartAsync(Guid orderId, Guid specialistId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return Result.Failure(BookingErrors.OrderNotFound.Description);

        if (order.SpecialistId != specialistId)
            return Result.Failure(BookingErrors.Forbidden.Description);

        order.Status = OrderStatus.Active;
        order.StartedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);
        return Result.Success();
    }

    public async Task<Result> CompleteAsync(Guid orderId, Guid specialistId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return Result.Failure(BookingErrors.OrderNotFound.Description);

        if (order.SpecialistId != specialistId)
            return Result.Failure(BookingErrors.Forbidden.Description);

        order.Status = OrderStatus.Completed;
        order.CompletedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);
        return Result.Success();
    }

    public async Task<Result> CancelAsync(Guid orderId, Guid clientId, string? reason = null)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
            return Result.Failure(BookingErrors.OrderNotFound.Description);

        if (order.ClientId != clientId)
            return Result.Failure(BookingErrors.Forbidden.Description);

        if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Canceled)
            return Result.Failure(BookingErrors.CannotCancelOrder.Description);

        order.Status = OrderStatus.Canceled;
        order.CanceledAt = DateTime.UtcNow;
        order.CancelReason = reason;

        await _orderRepository.UpdateAsync(order);

        _logger.LogInformation("Order {OrderId} canceled by client {ClientId}", orderId, clientId);

        return Result.Success();
    }

    public async Task<Result<RepeatServiceOrderDraft, Error>> RepeatAsync(Guid orderId, Guid clientId)
    {
        var originalOrder = await _orderRepository.GetByIdAsync(orderId);
        if (originalOrder == null)
            return Result.Failure<RepeatServiceOrderDraft, Error>(BookingErrors.OrderNotFound);

        if (originalOrder.ClientId != clientId)
            return Result.Failure<RepeatServiceOrderDraft, Error>(BookingErrors.Forbidden);

        var draft = new RepeatServiceOrderDraft
        {
            PetId = originalOrder.PetId,
            PetName = originalOrder.PetName,
            SpecialistId = originalOrder.SpecialistId,
            SpecialistName = originalOrder.SpecialistName,
            SpecialistSlug = originalOrder.SpecialistSlug,
            ServiceId = originalOrder.ServiceId,
            ServiceTitle = originalOrder.ServiceTitle,
            Price = originalOrder.Price,
            PriceUnit = originalOrder.PriceUnit,
            Comment = originalOrder.Comment,
        };

        _logger.LogInformation("Repeat draft created for order {OrderId}", orderId);

        return Result.Success<RepeatServiceOrderDraft, Error>(draft);
    }

    public async Task<Result<ServiceOrderReview, Error>> LeaveReviewAsync(Guid orderId, Guid clientId, int rating, string comment, List<string>? photos = null)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return Result.Failure<ServiceOrderReview, Error>(BookingErrors.OrderNotFound);

        if (order.ClientId != clientId)
            return Result.Failure<ServiceOrderReview, Error>(BookingErrors.Forbidden);

        if (order.Status != OrderStatus.Completed)
            return Result.Failure<ServiceOrderReview, Error>(BookingErrors.CannotReviewNotCompletedOrder);

        var review = new ServiceOrderReview
        {
            Id = Guid.NewGuid(),
            Rating = rating,
            Text = comment,
            CreatedAt = DateTime.UtcNow
        };

        order.Review = review;

        await _orderRepository.UpdateAsync(order);

        _logger.LogInformation("Review left for order {OrderId} by client {ClientId}", orderId, clientId);

        return Result.Success<ServiceOrderReview, Error>(review);
    }
}