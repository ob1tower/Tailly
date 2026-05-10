using CSharpFunctionalExtensions;
using MassTransit;
using Tailly.BookingService.Application.Dtos.Internal;
using Tailly.BookingService.Application.Errors;
using Tailly.BookingService.Application.Helpers;
using Tailly.BookingService.Application.Service.Interfaces;
using Tailly.BookingService.Core.Common;
using Tailly.BookingService.Core.Enums;
using Tailly.BookingService.Core.Models;
using Tailly.BookingService.Infrastructure.Repositories;
using Tailly.Contracts.Messages;

namespace Tailly.BookingService.Application.Service;

public class ServiceOrderService : IServiceOrderService
{
    private readonly IServiceOrderRepository _orderRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ServiceOrderService> _logger;

    public ServiceOrderService(IServiceOrderRepository orderRepository,
                               IPublishEndpoint publishEndpoint,
                               IHttpClientFactory httpClientFactory,
                               ILogger<ServiceOrderService> logger)
    {
        _orderRepository = orderRepository;
        _publishEndpoint = publishEndpoint;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<Result<ServiceOrder, Error>> CreateAsync(CreateServiceOrder model)
    {
        if (model == null)
            return Result.Failure<ServiceOrder, Error>(BookingErrors.InvalidOrder);

        if (model.StartAt <= DateTime.UtcNow)
            return Result.Failure<ServiceOrder, Error>(BookingErrors.InvalidOrderDate);

        if (model.EndAt.HasValue && model.EndAt <= model.StartAt)
        {
            return Result.Failure<ServiceOrder, Error>(BookingErrors.InvalidOrderDateRange);
        }

        var specialistClient = _httpClientFactory.CreateClient("specialist");
        var profileClient = _httpClientFactory.CreateClient("client-profile");

        var serviceResponse = await specialistClient.GetAsync($"/internal/services/{model.ServiceId}");

        if (!serviceResponse.IsSuccessStatusCode)
        {
            return Result.Failure<ServiceOrder, Error>(BookingErrors.InvalidService);
        }

        var serviceData = await serviceResponse.Content.ReadFromJsonAsync<ServiceInternalDto>();

        if (serviceData == null)
        {
            return Result.Failure<ServiceOrder, Error>(BookingErrors.InvalidService);
        }

        var petResponse = await profileClient.GetAsync($"/internal/pets/{model.PetId}");

        if (!petResponse.IsSuccessStatusCode)
        {
            return Result.Failure<ServiceOrder, Error>(BookingErrors.InvalidPet);
        }

        var petData = await petResponse.Content.ReadFromJsonAsync<PetInternalDto>();

        if (petData == null)
        {
            return Result.Failure<ServiceOrder, Error>(BookingErrors.InvalidPet);
        }

        if (petData.UserId != model.ClientId)
        {
            return Result.Failure<ServiceOrder, Error>(BookingErrors.InvalidClient);
        }

        if (serviceData.SpecialistId != model.SpecialistId)
        {
            return Result.Failure<ServiceOrder, Error>(BookingErrors.InvalidSpecialist);
        }

        var specialistResponse = await specialistClient.GetAsync($"/internal/specialists/{serviceData.SpecialistId}");

        if (!specialistResponse.IsSuccessStatusCode)
        {
            return Result.Failure<ServiceOrder, Error>(BookingErrors.InvalidSpecialist);
        }

        var specialistData = await specialistResponse.Content.ReadFromJsonAsync<SpecialistInternalDto>();

        if (specialistData == null)
        {
            return Result.Failure<ServiceOrder, Error>(BookingErrors.InvalidSpecialist);
        }

        var clientResponse = await profileClient.GetAsync($"/internal/clients/{model.ClientId}");

        if (!clientResponse.IsSuccessStatusCode)
        {
            return Result.Failure<ServiceOrder, Error>(BookingErrors.InvalidClient);
        }

        var clientData = await clientResponse.Content.ReadFromJsonAsync<ClientInternalDto>();

        if (clientData == null)
        {
            return Result.Failure<ServiceOrder, Error>(BookingErrors.InvalidClient);
        }

        var order = new ServiceOrder
        {
            Id = Guid.NewGuid(),
            Number = OrderNumberGenerator.Generate(),

            ClientId = clientData.UserId,
            ClientName = clientData.FullName,

            SpecialistId = specialistData.Id,
            SpecialistName = specialistData.FullName,
            SpecialistSlug = specialistData.Slug,

            PetId = petData.Id,
            PetName = petData.Name,

            ServiceId = serviceData.Id,
            ServiceTitle = serviceData.Name,
            Price = serviceData.Price,
            PriceUnit = Enum.Parse<PriceUnit>(serviceData.PriceUnit, true),

            StartAt = DateTimeHelper.NormalizeToUtc(model.StartAt),

            EndAt = model.EndAt.HasValue
                ? DateTimeHelper.NormalizeToUtc(model.EndAt.Value)
                : null,

            Comment = model.Comment,

            Status = OrderStatus.PendingConfirmation,
            CreatedAt = DateTime.UtcNow,

            ServiceSnapshot = new ServiceOrderServiceSnapshot
            {
                ServiceId = serviceData.Id,
                Title = serviceData.Name,
                Price = serviceData.Price,
                PriceUnit = Enum.Parse<PriceUnit>(
                    serviceData.PriceUnit,
                    true)
            }
        };

        await _orderRepository.AddAsync(order);

        _logger.LogInformation(
            "Service order created. OrderId: {OrderId}, ClientId: {ClientId}, SpecialistId: {SpecialistId}",
            order.Id,
            order.ClientId,
            order.SpecialistId);

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

        if (order.Status != OrderStatus.PendingConfirmation)
            return Result.Failure(BookingErrors.InvalidOrderStatus.Description);

        if (order.StartAt < DateTime.UtcNow)
            return Result.Failure(BookingErrors.OrderExpired.Description);

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

        if (order.Status != OrderStatus.Confirmed)
            return Result.Failure(BookingErrors.InvalidOrderStatus.Description);

        if (DateTime.UtcNow < order.StartAt.AddMinutes(-15))
            return Result.Failure(BookingErrors.TooEarlyToStartOrder.Description);

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

        if (order.Status != OrderStatus.Active)
            return Result.Failure(BookingErrors.InvalidOrderStatus.Description);

        if (order.StartedAt == null)
            return Result.Failure(BookingErrors.OrderNotStarted.Description);

        var completedOrdersCount = await _orderRepository.CountCompletedOrdersAsync(order.ClientId, order.SpecialistId, order.ServiceId);

        var isRepeatOrder = completedOrdersCount > 0;

        order.Status = OrderStatus.Completed;
        order.CompletedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);

        await _publishEndpoint.Publish(new OrderCompletedMessage
        {
            OrderId = order.Id,
            SpecialistId = order.SpecialistId,
            IsRepeatOrder = isRepeatOrder
        });

        return Result.Success();
    }

    public async Task<Result> CancelAsync(Guid orderId, Guid clientId, string? reason = null)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
            return Result.Failure(BookingErrors.OrderNotFound.Description);

        if (order.ClientId != clientId)
            return Result.Failure(BookingErrors.Forbidden.Description);

        if (order.Status == OrderStatus.Active)
            return Result.Failure(BookingErrors.CannotCancelActiveOrder.Description);

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

        if (originalOrder.Status != OrderStatus.Completed)
        {
            return Result.Failure<RepeatServiceOrderDraft, Error>(BookingErrors.CannotRepeatNotCompletedOrder);
        }

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

        if (order.Review != null)
            return Result.Failure<ServiceOrderReview, Error>(BookingErrors.ReviewAlreadyExists);

        var review = new ServiceOrderReview
        {
            Id = Guid.NewGuid(),
            Rating = rating,
            Text = comment,
            Photos = photos ?? [],
            CreatedAt = DateTime.UtcNow
        };

        await _orderRepository.AddReviewAsync(order.Id, review);

        await _publishEndpoint.Publish(new ReviewCreatedMessage
        {
            ReviewId = review.Id,

            SpecialistId = order.SpecialistId,
            OrderId = order.Id,

            AuthorName = order.ClientName,
            ServiceTitle = order.ServiceTitle,
            PetName = order.PetName,

            Rating = review.Rating,
            Text = review.Text,

            CreatedAt = review.CreatedAt,
            Photos = review.Photos
        });

        _logger.LogInformation("Review left for order {OrderId} by client {ClientId}", orderId, clientId);

        return Result.Success<ServiceOrderReview, Error>(review);
    }
}