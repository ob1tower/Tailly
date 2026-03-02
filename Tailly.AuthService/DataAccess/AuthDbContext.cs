using Microsoft.EntityFrameworkCore;
using Tailly.AuthService.Configurations;
using Tailly.AuthService.Entities;

namespace Tailly.AuthService.DataAccess;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
       : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<RoleEntity> Roles { get; set; }
    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }
}