using Microsoft.EntityFrameworkCore;
using Tailly.SpecialistService.Core.Entities.Details;
using Tailly.SpecialistService.Core.Entities.Gallery;
using Tailly.SpecialistService.Core.Entities.Specialist;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Reviews;
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
            .FirstOrDefaultAsync(x => x.Slug == slug);

        if (entity == null) return null;

        return SpecialistEntityMapper.ToModel(entity);
    }

    public async Task<Specialist?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Specialists
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null) return null;

        return SpecialistEntityMapper.ToModel(entity);
    }

    public async Task<Specialist?> GetFullProfileBySlugAsync(string slug)
    {
        var entity = await LoadFullProfileQuery()
            .FirstOrDefaultAsync(x => x.Slug == slug);

        if (entity == null) return null;

        return SpecialistEntityMapper.ToFullModel(entity);
    }

    public async Task<Specialist?> GetFullProfileByIdAsync(Guid id)
    {
        var entity = await LoadFullProfileQuery()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null) return null;

        return SpecialistEntityMapper.ToFullModel(entity);
    }

    private IQueryable<SpecialistEntity> LoadFullProfileQuery()
    {
        return _context.Specialists
            .Include(x => x.Details!).ThenInclude(d => d.PetSizes)
            .Include(x => x.Details!).ThenInclude(d => d.PetAges)
            .Include(x => x.Details!).ThenInclude(d => d.PetTypes)
            .Include(x => x.Services)
            .Include(x => x.Reviews)
            .Include(x => x.SpecialistGallery)
            .Include(x => x.Calendar!).ThenInclude(c => c.DayOverrides)
            .Include(x => x.Calendar!).ThenInclude(c => c.BookedSlots)
            .Include(x => x.Calendar!).ThenInclude(c => c.AvailabilityWindows)
            .Include(x => x.Calendar!).ThenInclude(c => c.BookingSettings)
            .AsNoTracking();
    }

    public async Task UpdateMainInfoAsync(Guid specialistId, string firstName, string lastName,
        string? middleName, string city, string district, string phone, string? avatarUrl)
    {
        var entity = await _context.Specialists.FirstOrDefaultAsync(x => x.Id == specialistId);
        if (entity == null) return;

        SpecialistEntityMapper.MapMainInfoToEntity(entity, firstName, lastName, middleName, city, district, phone, avatarUrl);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateDetailsAsync(Guid specialistId, Details details)
    {
        var existing = await _context.Details
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.SpecialistId == specialistId);

        if (existing == null)
        {
            var newDetails = new DetailsEntity
            {
                Id = Guid.NewGuid(),
                SpecialistId = specialistId,
                HousingType = details.HousingType,
                HasChildrenUnderTen = details.HasChildrenUnderTen,
                About = details.About
            };

            newDetails.PetSizes = details.PetSizes.Select(x => new PetSizeEntity
            {
                Id = Guid.NewGuid(),
                DetailsId = newDetails.Id,
                PetSize = x
            }).ToList();

            newDetails.PetAges = details.PetAges.Select(x => new PetAgeEntity
            {
                Id = Guid.NewGuid(),
                DetailsId = newDetails.Id,
                PetAge = x
            }).ToList();

            newDetails.PetTypes = details.PetTypes.Select(x => new PetTypeEntity
            {
                Id = Guid.NewGuid(),
                DetailsId = newDetails.Id,
                PetType = x
            }).ToList();

            await _context.Details.AddAsync(newDetails);
        }
        else
        {
            await _context.Details
                .Where(d => d.SpecialistId == specialistId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(d => d.HousingType, details.HousingType)
                    .SetProperty(d => d.HasChildrenUnderTen, details.HasChildrenUnderTen)
                    .SetProperty(d => d.About, details.About)
                );

            await _context.PetSizes.Where(p => p.DetailsId == existing.Id).ExecuteDeleteAsync();
            await _context.PetAges.Where(p => p.DetailsId == existing.Id).ExecuteDeleteAsync();
            await _context.PetTypes.Where(p => p.DetailsId == existing.Id).ExecuteDeleteAsync();

            if (details.PetSizes.Any())
                _context.PetSizes.AddRange(details.PetSizes.Select(x => new PetSizeEntity
                {
                    Id = Guid.NewGuid(),
                    DetailsId = existing.Id,
                    PetSize = x
                }));

            if (details.PetAges.Any())
                _context.PetAges.AddRange(details.PetAges.Select(x => new PetAgeEntity
                {
                    Id = Guid.NewGuid(),
                    DetailsId = existing.Id,
                    PetAge = x
                }));

            if (details.PetTypes.Any())
                _context.PetTypes.AddRange(details.PetTypes.Select(x => new PetTypeEntity
                {
                    Id = Guid.NewGuid(),
                    DetailsId = existing.Id,
                    PetType = x
                }));
        }

        var specialist = await _context.Specialists
            .Include(x => x.SpecialistGallery)
            .FirstOrDefaultAsync(x => x.Id == specialistId);

        if (specialist != null)
        {
            await _context.SpecialistGalleries
                .Where(g => g.SpecialistId == specialistId)
                .ExecuteDeleteAsync();

            if (details.SpecialistGallery?.Any() == true)
            {
                var newGallery = details.SpecialistGallery
                    .Select((x, index) => new SpecialistGalleryEntity
                    {
                        Id = Guid.NewGuid(),
                        SpecialistId = specialistId,
                        ImageUrl = x.ImageUrl,
                        Alt = x.Alt ?? "",
                        Order = index
                    })
                    .ToList();

                await _context.SpecialistGalleries.AddRangeAsync(newGallery);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<ServiceOffer?> GetServiceByIdAsync(Guid serviceId)
    {
        var entity = await _context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == serviceId);

        if (entity == null) return null;

        return SpecialistEntityMapper.ToServiceModel(entity);
    }

    public async Task AddServiceAsync(Guid specialistId, ServiceOffer service)
    {
        var entity = SpecialistEntityMapper.ToServiceEntity(specialistId, service);
        await _context.Services.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateServiceAsync(ServiceOffer service)
    {
        var entity = await _context.Services.FirstOrDefaultAsync(s => s.Id == service.Id);
        if (entity == null) return;

        SpecialistEntityMapper.UpdateServiceEntity(entity, service);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteServiceAsync(Guid serviceId)
    {
        var entity = await _context.Services.FindAsync(serviceId);
        if (entity != null)
        {
            _context.Services.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddReviewReplyAsync(Guid reviewId, string replyText)
    {
        var entity = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);
        if (entity == null) return;

        entity.ReplyText = replyText;
        entity.ReplyCreatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<Review?> GetReviewByIdAsync(Guid reviewId)
    {
        var entity = await _context.Reviews
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (entity == null) return null;

        return SpecialistEntityMapper.ToReviewModel(entity);
    }

    public async Task<bool> SlugExistsAsync(string slug)
    {
        return await _context.Specialists
            .AsNoTracking()
            .AnyAsync(s => s.Slug == slug);
    }

    public async Task AddAsync(Specialist specialist)
    {
        var entity = SpecialistEntityMapper.ToEntity(specialist);
        await _context.Specialists.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Specialist>> SearchAsync(string? cityQuery, string? districtQuery,
        string? serviceType, decimal? priceMin, decimal? priceMax, int page, int pageSize)
    {
        var query = _context.Specialists
            .Include(x => x.Services)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(cityQuery))
            query = query.Where(x => x.City.Contains(cityQuery));

        if (!string.IsNullOrWhiteSpace(districtQuery))
            query = query.Where(x => x.District.Contains(districtQuery));

        if (!string.IsNullOrWhiteSpace(serviceType) || priceMin.HasValue || priceMax.HasValue)
        {
            query = query.Where(x => x.Services.Any(s =>
                (!string.IsNullOrWhiteSpace(serviceType) ? s.Name.ToString() == serviceType : true) &&
                (!priceMin.HasValue || s.Price >= priceMin.Value) &&
                (!priceMax.HasValue || s.Price <= priceMax.Value)));
        }

        var specialists = await query
            .OrderByDescending(x => x.Rating)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return specialists.Select(SpecialistEntityMapper.ToModel).ToList();
    }

    public async Task<bool> HasServiceOfTypeAsync(Guid specialistId, ServiceType serviceType)
    {
        return await _context.Services
            .AsNoTracking()
            .AnyAsync(s => s.SpecialistId == specialistId && s.Name == serviceType);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return await _context.Specialists
            .AsNoTracking()
            .AnyAsync(s => s.Email.ToLower() == email.ToLower());
    }
}