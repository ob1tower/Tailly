using Tailly.SpecialistService.Application.Dtos.Requests.SpecialistApplication;
using Tailly.SpecialistService.Application.Dtos.Responses.SpecialistApplication;
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

        string lastName = nameParts.Length > 0 ? nameParts[0] : "";                  
        string firstName = nameParts.Length > 1 ? nameParts[1] : "";                
        string middleName = nameParts.Length > 2
            ? string.Join(" ", nameParts.Skip(2))
            : "";                                                                    

        return new SpecialistApplication
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? Guid.Empty,
            Email = request.Email?.Trim().ToLowerInvariant() ?? "",
            FirstName = firstName,     
            MiddleName = middleName,    
            LastName = lastName,      
            Phone = request.Phone?.Trim() ?? "",
            City = request.City?.Trim() ?? "",
            About = request.About?.Trim() ?? "",

            ExperienceYears = request.Questionnaire?.ExperienceYears ?? 0,
            AnimalTypes = request.Questionnaire?.AnimalTypes != null
                ? string.Join(",", request.Questionnaire.AnimalTypes)
                : "",
            ServiceFormats = request.Questionnaire?.ServiceFormats != null
                ? string.Join(",", request.Questionnaire.ServiceFormats)
                : "",

            CanGiveMedication = request.Questionnaire?.CanGiveMedication ?? false,
            CanHandleDifficultBehavior = request.Questionnaire?.CanHandleDifficultBehavior ?? false,
            CanTakeOvernightOrders = request.Questionnaire?.CanTakeOvernightOrders ?? false,
            HasOwnPets = request.Questionnaire?.HasOwnPets ?? false,
            HasPetFirstAidBasics = request.Questionnaire?.HasPetFirstAidBasics ?? false,
            HousingType = request.Questionnaire?.HousingType ?? "",
            DistrictPreferences = request.Questionnaire?.DistrictPreferences ?? "",
            SchedulePreferences = request.Questionnaire?.SchedulePreferences ?? "",
            PortfolioUrl = request.Questionnaire?.PortfolioUrl ?? "",
            Motivation = request.Questionnaire?.Motivation ?? "",
            AdditionalInfo = request.Questionnaire?.AdditionalInfo ?? "",

            Status = Core.Enums.SpecialistApplicationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static SpecialistApplicationListItemResponse ToListItem(SpecialistApplication app)
    {
        return new SpecialistApplicationListItemResponse
        {
            Id = app.Id,
            FullName = $"{app.LastName} {app.FirstName} {app.MiddleName}".Trim(),
            Email = app.Email,
            Phone = app.Phone,                                          
            City = app.City,
            Status = SpecialistEnumMapper.MapApplicationStatus(app.Status),
            CreatedAt = app.CreatedAt,
            UpdatedAt = app.UpdatedAt,                                  
            InterviewDate = app.InterviewDate?.ToString("yyyy-MM-ddTHH:mm"),
            ReviewComment = app.ReviewComment,                          
            ReviewedBy = app.ReviewedBy,                               
            About = app.About,
            CreatedSpecialistId = app.CreatedSpecialistId,               
            CreatedSpecialistSlug = app.CreatedSpecialistSlug,           
            SpecialistAccountCreatedAt = app.SpecialistAccountCreatedAt,          

            Questionnaire = new SpecialistApplicationQuestionnaireResponse
            {
                ExperienceYears = app.ExperienceYears,

                AnimalTypes = string.IsNullOrEmpty(app.AnimalTypes)
                ? new List<string>()
                : app.AnimalTypes.Split(',').Select(x => x.Trim()).ToList(),

                ServiceFormats = string.IsNullOrEmpty(app.ServiceFormats)
                ? new List<string>()
                : app.ServiceFormats.Split(',').Select(x => x.Trim()).ToList(),

                CanGiveMedication = app.CanGiveMedication,
                CanHandleDifficultBehavior = app.CanHandleDifficultBehavior,
                CanTakeOvernightOrders = app.CanTakeOvernightOrders,
                HasOwnPets = app.HasOwnPets,
                HasPetFirstAidBasics = app.HasPetFirstAidBasics,
                HousingType = app.HousingType,
                PortfolioUrl = app.PortfolioUrl,
                DistrictPreferences = app.DistrictPreferences,
                SchedulePreferences = app.SchedulePreferences,
                Motivation = app.Motivation,
                AdditionalInfo = app.AdditionalInfo
            }
        };
    }
}