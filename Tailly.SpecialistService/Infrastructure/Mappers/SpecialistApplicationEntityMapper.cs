using Tailly.SpecialistService.Core.Entities.Applications;
using Tailly.SpecialistService.Core.Models.Applications;

namespace Tailly.SpecialistService.Infrastructure.Mappers;

public static class SpecialistApplicationEntityMapper
{
    public static SpecialistApplicationEntity ToEntity(SpecialistApplication model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return new SpecialistApplicationEntity
        {
            Id = model.Id,
            UserId = model.UserId,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            MiddleName = model.MiddleName,
            Phone = model.Phone,
            City = model.City,
            About = model.About,
            ExperienceYears = model.ExperienceYears,
            AnimalTypes = model.AnimalTypes,
            ServiceFormats = model.ServiceFormats,
            CanGiveMedication = model.CanGiveMedication,
            CanHandleDifficultBehavior = model.CanHandleDifficultBehavior,
            CanTakeOvernightOrders = model.CanTakeOvernightOrders,
            HasOwnPets = model.HasOwnPets,
            HasPetFirstAidBasics = model.HasPetFirstAidBasics,
            HousingType = model.HousingType,
            DistrictPreferences = model.DistrictPreferences,
            SchedulePreferences = model.SchedulePreferences,
            PortfolioUrl = model.PortfolioUrl,
            Motivation = model.Motivation,
            AdditionalInfo = model.AdditionalInfo,
            PhotoUrl = model.PhotoUrl,
            Status = model.Status,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            ReviewComment = model.ReviewComment,
            ReviewedBy = model.ReviewedBy,
            InterviewNote = model.InterviewNote,
            InterviewDate = model.InterviewDate,
            RejectionReason = model.RejectionReason,
            CreatedSpecialistId = model.CreatedSpecialistId,
            CreatedSpecialistSlug = model.CreatedSpecialistSlug,
            SpecialistAccountCreatedAt = model.SpecialistAccountCreatedAt
        };
    }

    public static SpecialistApplication? ToDomain(this SpecialistApplicationEntity? entity)
    {
        if (entity == null)
            return null;

        return new SpecialistApplication
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Email = entity.Email,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            MiddleName = entity.MiddleName,
            Phone = entity.Phone,
            City = entity.City,
            About = entity.About,
            ExperienceYears = entity.ExperienceYears,
            AnimalTypes = entity.AnimalTypes,
            ServiceFormats = entity.ServiceFormats,
            CanGiveMedication = entity.CanGiveMedication,
            CanHandleDifficultBehavior = entity.CanHandleDifficultBehavior,
            CanTakeOvernightOrders = entity.CanTakeOvernightOrders,
            HasOwnPets = entity.HasOwnPets,
            HasPetFirstAidBasics = entity.HasPetFirstAidBasics,
            HousingType = entity.HousingType,
            DistrictPreferences = entity.DistrictPreferences,
            SchedulePreferences = entity.SchedulePreferences,
            PortfolioUrl = entity.PortfolioUrl,
            Motivation = entity.Motivation,
            AdditionalInfo = entity.AdditionalInfo,
            PhotoUrl = entity.PhotoUrl,
            Status = entity.Status,                   
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            ReviewComment = entity.ReviewComment,
            ReviewedBy = entity.ReviewedBy,
            InterviewNote = entity.InterviewNote,
            InterviewDate = entity.InterviewDate,
            RejectionReason = entity.RejectionReason,
            CreatedSpecialistId = entity.CreatedSpecialistId,
            CreatedSpecialistSlug = entity.CreatedSpecialistSlug,
            SpecialistAccountCreatedAt = entity.SpecialistAccountCreatedAt
        };
    }
}