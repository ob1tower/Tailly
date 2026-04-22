using Tailly.SpecialistService.Core.Common;

namespace Tailly.SpecialistService.Application.Errors;

public static class SpecialistApplicationErrors
{
    public static readonly Error NotFound =
        new("Application.NotFound", "The application was not found.");

    public static readonly Error CannotChangeRejected =
        new("Application.CannotChangeRejected", "You cannot assign an interview to a rejected application.");

    public static readonly Error CannotChangeApprove =
    new("Application.CannotChangeApprove", "You cannot approve an application that has been rejected.");

    public static readonly Error AlreadyProcessed =
        new("Application.AlreadyProcessed", "The application has already been processed.");

    public static readonly Error InvalidStatusTransition =
        new("Application.InvalidStatusTransition", "The application status cannot be changed.");

    public static readonly Error InvalidApplication =
        new("Application.Invalid", "Incorrect application data.");

    public static readonly Error SpecialistAlreadyExists =
        new("Application.SpecialistAlreadyExists", "The specialist's account has already been created for this application.");

    public static readonly Error ApplicationAlreadyExists =
        new("Application.AlreadyExists", "You already have an active application under consideration.");

    public static readonly Error AlreadySpecialist =
        new("Application.AlreadySpecialist", "You are already a specialist on the platform.");
}