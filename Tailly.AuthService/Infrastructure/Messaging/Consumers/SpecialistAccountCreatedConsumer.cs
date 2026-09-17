using MassTransit;
using Tailly.AuthService.Application.Service.Security;
using Tailly.AuthService.Application.Service.Security.Interfaces;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;
using Tailly.AuthService.Infrastructure.Service;
using Tailly.Contracts.Messages;

namespace Tailly.AuthService.Infrastructure.Messaging.Consumers;

public class SpecialistAccountCreatedConsumer : IConsumer<SpecialistAccountCreated>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHashingService _passwordHasher;
    private readonly IEmailSender _emailSender;
    private readonly SpecialistTemporaryPasswordService _temporaryPasswordService;
    private readonly ILogger<SpecialistAccountCreatedConsumer> _logger;

    public SpecialistAccountCreatedConsumer(IUsersRepository usersRepository,
                                            IPasswordHashingService passwordHasher,
                                            IEmailSender emailSender,
                                            SpecialistTemporaryPasswordService temporaryPasswordService,
                                            ILogger<SpecialistAccountCreatedConsumer> logger)
    {
        _usersRepository = usersRepository;
        _passwordHasher = passwordHasher;
        _emailSender = emailSender;
        _temporaryPasswordService = temporaryPasswordService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SpecialistAccountCreated> context)
    {
        var message = context.Message;

        try
        {
            _logger.LogInformation("Received SpecialistAccountCreated for Email: {Email}, SpecialistId: {SpecialistId}, SpecialistSlug: {SpecialistSlug}",
                message.Email, message.SpecialistId, message.Slug ?? "null");

            var existingUser = await _usersRepository.GetByEmailAsync(message.Email);

            if (existingUser != null)
            {
                _logger.LogInformation("User already exists. Adding Specialist role and updating links.");

                if (!existingUser.UserRoles.Any(x => x.Role == RoleType.Specialist && x.SoftDeletedAt == null))
                {
                    await _usersRepository.AddRoleAsync(existingUser.Id, (int)RoleType.Specialist);
                    
                    existingUser.UserRoles.Add(new UserRole
                    {
                        Role = RoleType.Specialist,
                        IsBlocked = false,
                        IsPermanentBlock = false,
                        SoftDeletedAt = null,
                        RestoreUntil = null
                    });

                    _logger.LogInformation("Added Specialist role to existing user {Email}", message.Email);
                }

                existingUser.SpecialistId = message.SpecialistId;
                existingUser.SpecialistSlug = message.Slug;
                existingUser.FirstName = message.FirstName;
                existingUser.LastName = message.LastName;
                existingUser.MiddleName = message.MiddleName;

                await _usersRepository.UpdateAsync(existingUser);

                _logger.LogInformation("Successfully linked Specialist to existing user {Email}", message.Email);

                var emailBody = $@"
                    <h2>You are now a Specialist on Tailly!</h2>
                    <p>Hello, {message.FirstName}!</p>
                    <p>Your account has been successfully updated — you now have specialist access.</p>
                    <p>You can continue using your current password.</p>
                    <p>Good luck with your work!</p>";

                await _emailSender.SendEmailAsync(
                    message.Email,
                    "You are now a Specialist — Tailly",
                    emailBody);

                await context.Publish(new SpecialistUserLinked
                {
                    SpecialistId = message.SpecialistId,
                    UserId = existingUser.Id
                });
            }
            else
            {
                var temporaryPassword = !string.IsNullOrEmpty(message.TemporaryPassword)
                    ? message.TemporaryPassword
                    : "Temp" + Guid.NewGuid().ToString("N").Substring(0, 8);

                await _temporaryPasswordService.SaveAsync(message.SpecialistId, temporaryPassword);

                var passwordHash = _passwordHasher.HashPassword(temporaryPassword);

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = message.Email,
                    PasswordHash = passwordHash,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    SpecialistId = message.SpecialistId,
                    SpecialistSlug = message.Slug,
                    FirstName = message.FirstName,
                    LastName = message.LastName,
                    MiddleName = message.MiddleName
                };

                await _usersRepository.AddAsync(user);
                await _usersRepository.AddRoleAsync(user.Id, (int)RoleType.Specialist);

                _logger.LogInformation("Created NEW Specialist user {Email} with SpecialistId {SpecialistId} and Slug {Slug}",
                    message.Email, message.SpecialistId, message.Slug);

                var emailBody = $@"
                    <h2>Your Specialist Account Has Been Created!</h2>
                    <p><strong>Login:</strong> <b>{message.Email}</b></p>
                    <p><strong>Temporary Password:</strong> <b>{temporaryPassword}</b></p>
                    <p>Please log in with this password and change it on your first login.</p>
                    <p>Good luck and welcome to Tailly!</p>";

                await _emailSender.SendEmailAsync(
                    message.Email,
                    "Specialist Account Created — Tailly",
                    emailBody);

                await context.Publish(new SpecialistUserLinked
                {
                    SpecialistId = message.SpecialistId,
                    UserId = user.Id
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process SpecialistAccountCreated for email {Email}", message.Email);
            throw;
        }
    }
}