using Tailly.BookingService.Application.Dtos.Requests;
using Tailly.BookingService.Application.Dtos.Responses;
using Tailly.BookingService.Core.Enums;
using Tailly.BookingService.Core.Models;

namespace Tailly.BookingService.Application.Mappers;

public static class ServiceOrderMapper
{
    public static CreateServiceOrder ToCreateModel(CreateServiceOrderRequest request)
    {
        return new CreateServiceOrder
        {
            SpecialistId = request.SpecialistId,
            ServiceId = request.ServiceId,
            PetId = request.PetId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Comment = request.Comment
        };
    }

    public static ServiceOrderResponse ToResponse(ServiceOrder model)
    {
        return new ServiceOrderResponse
        {
            Id = model.Id,
            Number = model.Number,
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
            PriceUnit = MapPriceUnit(model.PriceUnit),
            StartAt = model.StartAt,
            EndAt = model.EndAt,
            Comment = model.Comment,
            Status = MapStatus(model.Status),           
            CreatedAt = model.CreatedAt,
            ConfirmedAt = model.ConfirmedAt,
            StartedAt = model.StartedAt,
            CompletedAt = model.CompletedAt,
            CanceledAt = model.CanceledAt,
            CancelReason = model.CancelReason,
            Currency = "RUB",
            HasReview = model.Review != null
        };
    }

    private static string MapStatus(OrderStatus status) => status switch
    {
        OrderStatus.PendingConfirmation => "pending_confirmation",
        OrderStatus.Confirmed => "confirmed",
        OrderStatus.Active => "active",
        OrderStatus.Completed => "completed",
        OrderStatus.Canceled => "canceled",
        _ => status.ToString().ToLowerInvariant()
    };

    private static PriceUnit ParsePriceUnit(string? value)
    {
        return value?.ToLowerInvariant() switch
        {
            "hour" => PriceUnit.Hour,
            "day" => PriceUnit.Day,
            "service" => PriceUnit.Service,
            "walk" => PriceUnit.Walk,
            "visit" => PriceUnit.Visit,
            _ => PriceUnit.Service
        };
    }

    private static string MapPriceUnit(PriceUnit unit) => unit switch
    {
        PriceUnit.Hour => "hour",
        PriceUnit.Day => "day",
        PriceUnit.Service => "service",
        PriceUnit.Walk => "walk",
        PriceUnit.Visit => "visit",
        _ => "service"
    };
}