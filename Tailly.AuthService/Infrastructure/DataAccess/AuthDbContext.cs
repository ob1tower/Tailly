using Microsoft.EntityFrameworkCore;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Infrastructure.Configurations.Entities;

namespace Tailly.AuthService.Infrastructure.DataAccess;

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
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new AccountDeletionTokenConfiguration());
        modelBuilder.ApplyConfiguration(new AdminProfileConfiguration());
        modelBuilder.ApplyConfiguration(new AdminPasswordRecoveryConfiguration());
    }

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<RoleEntity> Roles { get; set; }
    public DbSet<UserRoleEntity> UserRoles { get; set; }
    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }
    public DbSet<AccountDeletionTokenEntity> AccountDeletionTokens { get; set; }
    public DbSet<AdminProfileEntity> AdminProfiles { get; set; }
    public DbSet<AdminPasswordRecoveryEntity> AdminPasswordRecoverys { get; set; }
}