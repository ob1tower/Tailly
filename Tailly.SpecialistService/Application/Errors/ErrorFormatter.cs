using FluentValidation.Results;

namespace Tailly.SpecialistService.Application.Errors;

public static class ErrorFormatter
{
    public static string[] Deserialize(IEnumerable<ValidationFailure> failures) =>
        failures
            .Select(f => f.ErrorMessage)
            .ToArray();
}