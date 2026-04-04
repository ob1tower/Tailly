using Microsoft.EntityFrameworkCore;
using Tailly.ClientProfileService.Core.Entities;
using Tailly.ClientProfileService.Core.Models;
using Tailly.ClientProfileService.Infrastructure.DataAccess;
using Tailly.ClientProfileService.Infrastructure.Repositories.Interfaces;

namespace Tailly.ClientProfileService.Infrastructure.Repositories;

public class PetRepository : IPetRepository
{
    private readonly ClientProfileDbContext _context;

    public PetRepository(ClientProfileDbContext context)
    {
        _context = context;
    }

    public async Task<List<Pet>> GetByClientIdAsync(Guid clientId)
    {
        var entities = await _context.Pets
            .AsNoTracking()
            .Where(x => x.ClientId == clientId)
            .ToListAsync();

        return entities.Select(x => new Pet
        {
            Id = x.Id,
            ClientId = x.ClientId,
            Name = x.Name,
            PhotoUrl = x.PhotoUrl,
            Type = x.Type,
            BreedId = x.BreedId,
            AgeYears = x.AgeYears,
            AgeMonths = x.AgeMonths,
            Size = x.Size,
            Gender = x.Gender,
            ToOtherPets = x.ToOtherPets,
            ToKidsUnder10 = x.ToKidsUnder10,
            StaysHomeAlone = x.StaysHomeAlone,
            Vaccinated = x.Vaccinated,
            Notes = x.Notes,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        }).ToList();
    }

    public async Task<Pet?> GetByIdAsync(Guid id)
    {
        var entities = await _context.Pets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entities == null)
            return null;

        return new Pet
        {
            Id = entities.Id,
            ClientId = entities.ClientId,
            Name = entities.Name,
            PhotoUrl = entities.PhotoUrl,
            Type = entities.Type,
            BreedId = entities.BreedId,
            AgeYears = entities.AgeYears,
            AgeMonths = entities.AgeMonths,
            Size = entities.Size,
            Gender = entities.Gender,
            ToOtherPets = entities.ToOtherPets,
            ToKidsUnder10 = entities.ToKidsUnder10,
            StaysHomeAlone = entities.StaysHomeAlone,
            Vaccinated = entities.Vaccinated,
            Notes = entities.Notes,
            CreatedAt = entities.CreatedAt,
            UpdatedAt = entities.UpdatedAt
        };
    }

    public async Task AddAsync(Pet pet)
    {
        var entity = new PetEntity
        {
            Id = pet.Id,
            ClientId = pet.ClientId,
            Name = pet.Name,
            PhotoUrl = pet.PhotoUrl,
            Type = pet.Type,
            BreedId = pet.BreedId,
            AgeYears = pet.AgeYears,
            AgeMonths = pet.AgeMonths,
            Size = pet.Size,
            Gender = pet.Gender,
            ToOtherPets = pet.ToOtherPets,
            ToKidsUnder10 = pet.ToKidsUnder10,
            StaysHomeAlone = pet.StaysHomeAlone,
            Vaccinated = pet.Vaccinated,
            Notes = pet.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Pets.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Pet pet)
    {
        var entity = await _context.Pets
            .FirstOrDefaultAsync(x => x.Id == pet.Id);

        if (entity == null)
            return;

        entity.Name = pet.Name;
        entity.PhotoUrl = pet.PhotoUrl;
        entity.Type = pet.Type;
        entity.BreedId = pet.BreedId;
        entity.AgeYears = pet.AgeYears;
        entity.AgeMonths = pet.AgeMonths;
        entity.Size = pet.Size;
        entity.Gender = pet.Gender;
        entity.ToOtherPets = pet.ToOtherPets;
        entity.ToKidsUnder10 = pet.ToKidsUnder10;
        entity.StaysHomeAlone = pet.StaysHomeAlone;
        entity.Vaccinated = pet.Vaccinated;
        entity.Notes = pet.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Pets
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity == null)
            return;

        _context.Pets.Remove(entity);
        await _context.SaveChangesAsync();
    }
}