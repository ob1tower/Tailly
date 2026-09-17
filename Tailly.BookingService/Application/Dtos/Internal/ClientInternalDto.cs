namespace Tailly.BookingService.Application.Dtos.Internal;

public sealed class ClientInternalDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = default!;
}