namespace Tailly.SpecialistService.Application.Dtos.Requests.SpecialistApplication;

public sealed class CreateSpecialistApplicationRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string About { get; set; } = string.Empty;
    public SpecialistApplicationQuestionnaire Questionnaire { get; set; } = new();
}