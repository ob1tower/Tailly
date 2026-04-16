using Microsoft.EntityFrameworkCore;
using Tailly.AuthService.Application.Service.Security.Interfaces;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Infrastructure.DataAccess;

namespace Tailly.AuthService.Infrastructure.Seed;

public static class SeedExtensions
{
    public static async Task SeedSuperAdminAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHashingService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Seed");

        var exists = await context.Users
            .Include(u => u.UserRoles)
            .AnyAsync(u => u.UserRoles.Any(r => r.RoleId == (int)RoleType.SuperAdmin));

        if (exists)
        {
            logger.LogInformation("SuperAdmin already exists");
            return;
        }

        var email = "superadmin@tailly.com";
        var password = "12345678";

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHasher.HashPassword(password),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Users.AddAsync(user);

        await context.UserRoles.AddAsync(new UserRoleEntity
        {
            UserId = user.Id,
            RoleId = (int)RoleType.SuperAdmin
        });

        await context.SaveChangesAsync();

        logger.LogWarning("Super Admin created: {Email}", email);
    }
}