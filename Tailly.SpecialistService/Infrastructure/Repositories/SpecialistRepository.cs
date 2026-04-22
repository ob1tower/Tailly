using Microsoft.EntityFrameworkCore;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Specialist;
using Tailly.SpecialistService.Infrastructure.DataAccess;
using Tailly.SpecialistService.Infrastructure.Mappers;
using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Infrastructure.Repositories;

public class SpecialistRepository : ISpecialistRepository
{
    private readonly SpecialistDbContext _context;

    public SpecialistRepository(SpecialistDbContext context)
    {
        _context = context;
    }

    public async Task<Specialist?> GetBySlugAsync(string slug)
    {
        var entity = await _context.Specialists
            .AsNoTracking()
            .Include(x => x.Details)
            .Include(x => x.Services)
            .Include(x => x.Reviews)
            .Include(x => x.Gallery)
            .Include(x => x.Availabilities).ThenInclude(x => x.Services)
            .Include(x => x.BookedSlots).ThenInclude(x => x.Services)
            .Include(x => x.PetTypes)
            .Include(x => x.PetSizes)
            .Include(x => x.PetAges)
            .Include(x => x.Advantages)
            .Include(x => x.AvailabilityWeekdays)
            .FirstOrDefaultAsync(x => x.Slug == slug);

        return entity.ToDomain();
    }

    public async Task<Specialist?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Specialists
            .AsNoTracking()
            .Include(x => x.Details)
            .Include(x => x.Services)
            .Include(x => x.Reviews)
            .Include(x => x.Gallery)
            .Include(x => x.Availabilities).ThenInclude(x => x.Services)
            .Include(x => x.BookedSlots).ThenInclude(x => x.Services)
            .Include(x => x.PetTypes)
            .Include(x => x.PetSizes)
            .Include(x => x.PetAges)
            .Include(x => x.Advantages)
            .Include(x => x.AvailabilityWeekdays)
            .FirstOrDefaultAsync(x => x.Id == id);

        return entity?.ToDomain();
    }

    public async Task<(List<Specialist> specialists, int total)> GetAllAsync(string? cityQuery, string? districtQuery, Guid? serviceId, decimal? priceMin, decimal? priceMax,
                                                                              int? experienceMinYears, bool hasReviewsOnly, SpecialistSort sort, int page, int limit)
    {
        var query = _context.Specialists
            .Include(x => x.Services)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(cityQuery))
        {
            var city = cityQuery.ToLower();
            query = query.Where(x => x.City.ToLower().Contains(city));
        }

        if (!string.IsNullOrWhiteSpace(districtQuery))
        {
            var district = districtQuery.ToLower();
            query = query.Where(x => x.District.ToLower().Contains(district));
        }

        if (experienceMinYears.HasValue)
            query = query.Where(x => x.ExperienceYears >= experienceMinYears.Value);

        if (hasReviewsOnly)
            query = query.Where(x => x.ReviewsCount > 0);

        if (priceMin.HasValue)
            query = query.Where(x => x.Services.Any(s => s.Price >= priceMin.Value));

        if (priceMax.HasValue)
            query = query.Where(x => x.Services.Any(s => s.Price <= priceMax.Value));

        if (serviceId.HasValue)
            query = query.Where(x => x.Services.Any(s => s.Id == serviceId.Value));

        query = sort switch
        {
            SpecialistSort.PriceAsc => query.OrderBy(x => x.Services.Min(s => s.Price)),
            SpecialistSort.PriceDesc => query.OrderByDescending(x => x.Services.Min(s => s.Price)),
            SpecialistSort.RatingDesc => query.OrderByDescending(x => x.Rating),
            _ => query.OrderByDescending(x => x.Rating)
        };

        var total = await query.CountAsync();

        var entities = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var specialists = entities
            .Select(SpecialistEntityMapper.ToDomainRequired)
            .ToList();

        return (specialists, total);
    }

    public async Task AddAsync(Specialist specialist)
    {
        var entity = SpecialistEntityMapper.ToEntity(specialist);

        await _context.Specialists.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> SlugExistsAsync(string slug)
    {
        return await _context.Specialists
            .AsNoTracking()
            .AnyAsync(s => s.Slug == slug);
    }

    public async Task UpdateAsync(Specialist specialist)
    {
        var entity = await _context.Specialists
            .Include(x => x.Details)
            .Include(x => x.Services)
            .Include(x => x.PetTypes)
            .Include(x => x.PetSizes)
            .Include(x => x.PetAges)
            .Include(x => x.Advantages)
            .FirstOrDefaultAsync(x => x.Id == specialist.Id);

        if (entity == null)
            return;

        SpecialistEntityMapper.UpdateEntity(entity, specialist);

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return await _context.Specialists
            .AnyAsync(s => s.Email.ToLower() == email.ToLower());
    }
}