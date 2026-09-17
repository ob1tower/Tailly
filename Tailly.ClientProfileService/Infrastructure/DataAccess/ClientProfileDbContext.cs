using Microsoft.EntityFrameworkCore;
using Tailly.ClientProfileService.Core.Entities;
using Tailly.ClientProfileService.Infrastructure.Configurations.Entities;

namespace Tailly.ClientProfileService.Infrastructure.DataAccess;

public class ClientProfileDbContext : DbContext
{
    public ClientProfileDbContext(DbContextOptions<ClientProfileDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new ClientProfileEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PetEntityConfiguration());
        modelBuilder.ApplyConfiguration(new BreedEntityConfiguration());

        BreedSeedData.Seed(modelBuilder);
    }

    public DbSet<ClientProfileEntity> ClientProfiles { get; set; }
    public DbSet<PetEntity> Pets { get; set; }
    public DbSet<BreedEntity> Breeds { get; set; }
}