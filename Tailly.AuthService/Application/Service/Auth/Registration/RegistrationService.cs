using CSharpFunctionalExtensions;
using MassTransit;
using Tailly.AuthService.Application.Errors;
using Tailly.AuthService.Application.Mappers;
using Tailly.AuthService.Application.Service.Auth.Common;
using Tailly.AuthService.Application.Service.Security.Interfaces;
using Tailly.AuthService.Application.Service.Security.Otp;
using Tailly.AuthService.Application.Service.Tokens.Interfaces;
using Tailly.AuthService.Core.Common;
using Tailly.AuthService.Core.Entities;
using Tailly.AuthService.Core.Enums;
using Tailly.AuthService.Core.Models;
using Tailly.AuthService.Infrastructure.Messaging.Messages;
using Tailly.AuthService.Infrastructure.Repositories.Interfaces;
using Tailly.Contracts.Messages;

namespace Tailly.AuthService.Application.Service.Auth.Registration;

public class RegistrationService : IRegistrationService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHashingService _passwordHasher;
    private readonly IJwtTokenService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IVerificationCodeService _verificationCodeService;
    private readonly IPendingRegistrationService _pendingRegistrationService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<RegistrationService> _logger;

    public RegistrationService(IUsersRepository usersRepository,
                               IRefreshTokenRepository refreshTokenRepository,
                               IPasswordHashingService passwordHasher,
                               IJwtTokenService jwtService,
                               IRefreshTokenService refreshTokenService,
                               IVerificationCodeService verificationCodeService,
                               IPendingRegistrationService pendingRegistrationService,
                               IPublishEndpoint publishEndpoint,
                               ILogger<RegistrationService> logger)
    {
        _usersRepository = usersRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _verificationCodeService = verificationCodeService;
        _pendingRegistrationService = pendingRegistrationService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }



    public async Task<Result<string, Error>> StartRegisterAsync(string email, string password)
    {
        email = email?.Trim().ToLowerInvariant()
            ?? throw new ArgumentNullException(nameof(email));

        var exists = await _usersRepository.ExistsAsync(email);

        if (exists)
        {
            _logger.LogWarning("Registration failed. User already exists: {Email}", email);
            return Result.Failure<string, Error>(AuthErrors.UserAlreadyExists);
        }

        var passwordHash = _passwordHasher.HashPassword(password);

        var registrationId = await _pendingRegistrationService.CreateAsync(email, passwordHash);

        var code = VerificationCodeGenerator.GenerateCode();

        await _verificationCodeService.SetCodeAsync(email, code, "register");

        await _publishEndpoint.Publish(new SendEmailMessage
        {
            To = email,
            Subject = "Registration confirmation",
            Body = $"""
            <h2>Registration</h2>
            <p>Your verification code:</p>
            <h1>{code}</h1>
            <p>This code will expire in 15 minutes.</p>
            """,
            Purpose = "register"
        });

        _logger.LogInformation("Registration started for {Email}", email);

        return Result.Success<string, Error>(registrationId);
    }

    public async Task<Result<string, Error>> VerifyRegisterAsync(string registrationId, string code)
    {
        var data = await _pendingRegistrationService.GetAsync(registrationId);

        if (data == null)
        {
            _logger.LogWarning("Verify failed. Registration not found: {RegistrationId}", registrationId);
            return Result.Failure<string, Error>(AuthErrors.RegistrationNotFound);
        }

        var (email, _) = data.Value;

        var result = await _verificationCodeService.VerifyCodeAsync(email, code, "register");

        if (!result.Success)
        {
            _logger.LogWarning("Verify failed. Invalid code: {Email}", email);
            return Result.Failure<string, Error>(AuthErrors.InvalidVerificationCode);
        }

        var verificationToken = Guid.NewGuid().ToString();

        await _verificationCodeService.SetCodeAsync(
            verificationToken,
            "ok",
            "register-token");

        await _pendingRegistrationService.AttachTokenAsync(registrationId, verificationToken);

        _logger.LogInformation("Email verified for {Email}", email);

        return Result.Success<string, Error>(verificationToken);
    }

    public async Task<Result<AuthResult, Error>> CompleteRegisterAsync(string verificationToken, string firstName, string lastName,
                                                                       string? middleName, string? cityName, string cityId)
    {
        var tokenValid = await _verificationCodeService.VerifyCodeAsync(
            verificationToken, "ok", "register-token");

        if (!tokenValid.Success)
            return Result.Failure<AuthResult, Error>(AuthErrors.InvalidVerificationToken);

        var data = await _pendingRegistrationService.GetByTokenAsync(verificationToken);
        if (data == null)
            return Result.Failure<AuthResult, Error>(AuthErrors.RegistrationNotFound);

        var (email, passwordHash) = data.Value;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        await _usersRepository.AddAsync(user);
        await _usersRepository.AddRoleAsync(user.Id, (int)RoleType.Client);

        await _pendingRegistrationService.RemoveByTokenAsync(verificationToken);

        var (accessToken, accessExpires) = await _jwtService.CreateAccessTokenAsync(new UserEntity
        {
            Id = user.Id,
            Email = user.Email,
            UserRoles = new List<UserRoleEntity>
        {
            new UserRoleEntity { UserId = user.Id, RoleId = (int)RoleType.Client }
        }
        });

        var (rawRefreshToken, hashedRefreshToken) = _refreshTokenService.GenerateToken();
        var refreshExpires = _refreshTokenService.GetRefreshTokenExpiryDate();

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = hashedRefreshToken,
            UserId = user.Id,
            Created = DateTime.UtcNow,
            Expires = refreshExpires
        });

        var result = new AuthResult
        {
            AccessToken = accessToken,
            AccessTokenExpires = accessExpires,
            RefreshToken = rawRefreshToken,
            RefreshTokenExpires = refreshExpires,
            User = AuthMapper.ToDto(user, RoleType.Client)
        };

        _logger.LogInformation("User registered successfully: {UserId}", user.Id);

        await _publishEndpoint.Publish(new UserRegisteredMessage
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = firstName,
            LastName = lastName,
            MiddleName = middleName,
            CityName = cityName,
            CityId = cityId
        });

        return Result.Success<AuthResult, Error>(result);
    }
}
