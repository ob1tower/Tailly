using Microsoft.EntityFrameworkCore;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Applications;
using Tailly.SpecialistService.Infrastructure.DataAccess;
using Tailly.SpecialistService.Infrastructure.Mappers;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Infrastructure.Repositories;

public class SpecialistApplicationRepository : ISpecialistApplicationRepository
{
    private readonly SpecialistDbContext _context;

    public SpecialistApplicationRepository(SpecialistDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(SpecialistApplication application)
    {
        var entity = SpecialistApplicationEntityMapper.ToEntity(application);
        await _context.SpecialistApplications.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<SpecialistApplication?> GetByIdAsync(Guid id)
    {
        var entity = await _context.SpecialistApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        return entity?.ToDomain();
    }

    public async Task<(List<SpecialistApplication> items, int total)> GetAllAsync(
        int page, int limit, SpecialistApplicationStatus? status = null)
    {
        var query = _context.SpecialistApplications
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var total = await query.CountAsync();

        var entities = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var items = entities
            .Select(x => x.ToDomain()!)
            .ToList();

        return (items, total);
    }

    public async Task UpdateAsync(SpecialistApplication application)
    {
        var entity = await _context.SpecialistApplications
            .FirstOrDefaultAsync(x => x.Id == application.Id);

        if (entity == null)
            return;

        entity.FirstName = application.FirstName;
        entity.LastName = application.LastName;
        entity.MiddleName = application.MiddleName;
        entity.Phone = application.Phone;
        entity.City = application.City;
        entity.About = application.About;
        entity.ExperienceYears = application.ExperienceYears;
        entity.AnimalTypes = application.AnimalTypes;
        entity.ServiceFormats = application.ServiceFormats;
        entity.CanGiveMedication = application.CanGiveMedication;
        entity.CanHandleDifficultBehavior = application.CanHandleDifficultBehavior;
        entity.CanTakeOvernightOrders = application.CanTakeOvernightOrders;
        entity.HasOwnPets = application.HasOwnPets;
        entity.HasPetFirstAidBasics = application.HasPetFirstAidBasics;
        entity.HousingType = application.HousingType;
        entity.DistrictPreferences = application.DistrictPreferences;
        entity.SchedulePreferences = application.SchedulePreferences;
        entity.PortfolioUrl = application.PortfolioUrl;
        entity.Motivation = application.Motivation;
        entity.AdditionalInfo = application.AdditionalInfo;
        entity.PhotoUrl = application.PhotoUrl;
        entity.Status = application.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.ReviewComment = application.ReviewComment;
        entity.ReviewedBy = application.ReviewedBy;
        entity.InterviewNote = application.InterviewNote;
        entity.InterviewDate = application.InterviewDate;
        entity.RejectionReason = application.RejectionReason;
        entity.CreatedSpecialistId = application.CreatedSpecialistId;
        entity.CreatedSpecialistSlug = application.CreatedSpecialistSlug;
        entity.SpecialistAccountCreatedAt = application.SpecialistAccountCreatedAt;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.SpecialistApplications
            .AnyAsync(x => x.Id == id);
    }

    public async Task<bool> HasActiveApplicationAsync(string email)
    {
        return await _context.SpecialistApplications
            .AnyAsync(a => a.Email.ToLower() == email.ToLower() &&
                           a.Status == SpecialistApplicationStatus.Pending);
    }

    public async Task<bool> HasInterviewConflictAsync(string reviewedBy, DateTime interviewDate)
    {
        if (string.IsNullOrWhiteSpace(reviewedBy))
            return false;

        var start = interviewDate.AddMinutes(-59);
        var end = interviewDate.AddMinutes(59);

        return await _context.SpecialistApplications
            .AnyAsync(a =>
                a.ReviewedBy == reviewedBy &&
                a.InterviewDate.HasValue &&
                a.InterviewDate >= start &&
                a.InterviewDate <= end &&
                a.Status == SpecialistApplicationStatus.InterviewScheduled);
    }
}