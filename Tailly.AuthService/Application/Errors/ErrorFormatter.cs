using FluentValidation.Results;

namespace Tailly.AuthService.Application.Errors;

public static class ErrorFormatter
{
    public static string[] Deserialize(IEnumerable<ValidationFailure> failures) =>
        failures
            .Select(f => f.ErrorMessage)
            .ToArray();
}