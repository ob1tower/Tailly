namespace Tailly.SpecialistService.Infrastructure.Configurations.Options;

public sealed class RabbitMqSettings
{
    public required string Host { get; set; }
    public required int Port { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}