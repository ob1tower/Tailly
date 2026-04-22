using Tailly.SpecialistService.Application.Dtos.Requests.SpecialistApplication;
using Tailly.SpecialistService.Application.Dtos.Responses.SpecialistApplication;
using Tailly.SpecialistService.Core.Enums;
using Tailly.SpecialistService.Core.Models.Applications;

namespace Tailly.SpecialistService.Application.Mappers;

public static class SpecialistApplicationMapper
{
    public static SpecialistApplication ToModel(CreateSpecialistApplicationRequest request, Guid? userId)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var nameParts = string.IsNullOrWhiteSpace(request.FullName)
            ? Array.Empty<string>()
            : request.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        string firstName = string.Empty;
        string middleName = string.Empty;
        string lastName = string.Empty;

        switch (nameParts.Length)
        {
            case 0:
                break;
            case 1:
                lastName = nameParts[0];          
                break;
            case 2:
                lastName = nameParts[0];
                firstName = nameParts[1];
                break;
            default: 
                lastName = nameParts[0];
                firstName = nameParts[1];
                middleName = string.Join(" ", nameParts.Skip(2));
                break;
        }

        return new SpecialistApplication
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? Guid.Empty,
            Email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            Phone = request.Phone?.Trim() ?? string.Empty,
            City = request.City?.Trim() ?? string.Empty,
            About = request.About?.Trim() ?? string.Empty,
            PhotoUrl = null,

            ExperienceYears = ParseExperienceYears(request.Questionnaire?.ExperienceYears),
            ServicesWanted = request.Questionnaire?.ServiceFormats != null
                ? string.Join(",", request.Questionnaire.ServiceFormats)
                : string.Empty,

            Status = SpecialistApplicationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static int ParseExperienceYears(string? experience)
    {
        if (string.IsNullOrWhiteSpace(experience))
            return 0;

        var match = System.Text.RegularExpressions.Regex.Match(experience, @"\d+");
        return match.Success ? int.Parse(match.Value) : 0;
    }

    public static SpecialistApplicationListItemResponse ToListItem(SpecialistApplication app)
    {
        return new SpecialistApplicationListItemResponse
        {
            Id = app.Id,
            FullName = $"{app.LastName} {app.FirstName} {app.MiddleName}".Trim(),
            Email = app.Email,
            City = app.City,
            Status = app.Status.ToString(),
            CreatedAt = app.CreatedAt
        };
    }
}