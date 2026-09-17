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
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");

        var email = "superadmin@tailly.com";
        if (await context.Users.AnyAsync(u => u.Email == email)) return;

        var adminId = Guid.NewGuid();  

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHasher.HashPassword("12345678"),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            FirstName = "Алексей",
            LastName = "Морозов",
            AdminId = adminId                   
        };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var profile = new AdminProfileEntity
        {
            Id = adminId,                        
            UserId = user.Id,
            BirthDate = new DateTime(1988, 4, 12, 0, 0, 0, DateTimeKind.Utc),
            Phone = "+79161234567",
            Department = AdminDepartment.Administration
        };

        await context.AdminProfiles.AddAsync(profile);
        await context.SaveChangesAsync();

        await context.UserRoles.AddAsync(new UserRoleEntity
        {
            UserId = user.Id,
            RoleId = (int)RoleType.SuperAdmin
        });

        await context.SaveChangesAsync();

        logger.LogWarning("Super Admin created: {Email}", email);
    }
}