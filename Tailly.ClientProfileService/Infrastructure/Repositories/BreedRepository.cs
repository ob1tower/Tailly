using Microsoft.EntityFrameworkCore;
using Tailly.ClientProfileService.Core.Models;
using Tailly.ClientProfileService.Infrastructure.DataAccess;
using Tailly.ClientProfileService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ClientProfileService.Infrastructure.Repositories;

public class BreedRepository : IBreedRepository
{
    private readonly ClientProfileDbContext _context;

    public BreedRepository(ClientProfileDbContext context)
    {
        _context = context;
    }

    public async Task<List<Breed>> GetAllAsync()
    {
        var entities = await _context.Breeds
            .AsNoTracking()
            .ToListAsync();

        return entities.Select(x => new Breed
        {
            Id = x.Id,
            Type = x.Type,
            Title = x.Title,
            Description = x.Description
        }).ToList();
    }
}