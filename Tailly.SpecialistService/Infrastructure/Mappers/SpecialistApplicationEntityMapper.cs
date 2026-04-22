using Tailly.SpecialistService.Core.Entities.Applications;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Applications;

namespace Tailly.SpecialistService.Infrastructure.Mappers;

public static class SpecialistApplicationEntityMapper
{
    public static SpecialistApplicationEntity ToEntity(SpecialistApplication model)
    {
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
            ServicesWanted = model.ServicesWanted,
            PhotoUrl = model.PhotoUrl,
            Status = model.Status,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            InterviewNote = model.InterviewNote,
            InterviewDate = model.InterviewDate,
            RejectionReason = model.RejectionReason
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
            ServicesWanted = entity.ServicesWanted,
            PhotoUrl = entity.PhotoUrl,
            Status = entity.Status ?? SpecialistApplicationStatus.Pending,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            InterviewNote = entity.InterviewNote,
            InterviewDate = entity.InterviewDate,
            RejectionReason = entity.RejectionReason
        };
    }

    public static SpecialistApplication ToDomainRequired(this SpecialistApplicationEntity entity)
    {
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
            ServicesWanted = entity.ServicesWanted,
            PhotoUrl = entity.PhotoUrl,
            Status = entity.Status ?? SpecialistApplicationStatus.Pending,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            InterviewNote = entity.InterviewNote,
            InterviewDate = entity.InterviewDate,
            RejectionReason = entity.RejectionReason
        };
    }
}