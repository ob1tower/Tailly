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

    public async Task<(List<SpecialistApplication> items, int total)> GetAllAsync(int page, int limit)
    {
        var query = _context.SpecialistApplications
            .AsNoTracking()
            .AsQueryable();

        var total = await query.CountAsync();

        var entities = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var items = entities
            .Select(x => x.ToDomainRequired())
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
        entity.ServicesWanted = application.ServicesWanted;
        entity.PhotoUrl = application.PhotoUrl;
        entity.Status = application.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.InterviewNote = application.InterviewNote;
        entity.InterviewDate = application.InterviewDate;
        entity.RejectionReason = application.RejectionReason;

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
}