using Tailly.BookingService.Core.Entities;
using Tailly.BookingService.Core.Models;

namespace Tailly.BookingService.Infrastructure.Mappers;

public static class ServiceOrderEntityMapper
{
    public static ServiceOrderEntity ToEntity(ServiceOrder model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        var entity = new ServiceOrderEntity
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
            PriceUnit = model.PriceUnit,

            StartAt = model.StartAt,
            EndAt = model.EndAt,

            Comment = model.Comment,

            Status = model.Status,
            CreatedAt = model.CreatedAt,
            ConfirmedAt = model.ConfirmedAt,
            StartedAt = model.StartedAt,
            CompletedAt = model.CompletedAt,
            CanceledAt = model.CanceledAt,
            CancelReason = model.CancelReason
        };

        if (model.Review != null)
        {
            entity.Review = new ServiceOrderReviewEntity
            {
                Id = model.Review.Id,
                OrderId = model.Id,
                Rating = model.Review.Rating,
                Text = model.Review.Text,
                Photos = model.Review.Photos ?? new List<string>(),
                CreatedAt = model.Review.CreatedAt
            };
        }

        return entity;
    }

    public static ServiceOrder ToDomain(ServiceOrderEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return new ServiceOrder
        {
            Id = entity.Id,
            Number = entity.Number,

            ClientId = entity.ClientId,
            ClientName = entity.ClientName,

            SpecialistId = entity.SpecialistId,
            SpecialistName = entity.SpecialistName,
            SpecialistSlug = entity.SpecialistSlug,

            PetId = entity.PetId,
            PetName = entity.PetName,

            ServiceId = entity.ServiceId,
            ServiceTitle = entity.ServiceTitle,
            Price = entity.Price,
            PriceUnit = entity.PriceUnit,

            StartAt = entity.StartAt,
            EndAt = entity.EndAt,

            Comment = entity.Comment,

            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            ConfirmedAt = entity.ConfirmedAt,
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt,
            CanceledAt = entity.CanceledAt,
            CancelReason = entity.CancelReason,

            Review = entity.Review == null ? null : new ServiceOrderReview
            {
                Id = entity.Review.Id,
                Rating = entity.Review.Rating,
                Text = entity.Review.Text,
                Photos = entity.Review.Photos ?? new List<string>(),
                CreatedAt = entity.Review.CreatedAt
            }
        };
    }
}