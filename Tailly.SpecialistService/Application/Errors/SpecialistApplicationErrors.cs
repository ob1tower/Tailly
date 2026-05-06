using Tailly.SpecialistService.Core.Common;

namespace Tailly.SpecialistService.Application.Errors;

public static class SpecialistApplicationErrors
{
    public static readonly Error NotFound =
        new("Application.NotFound", "The application was not found.");

    public static readonly Error CannotChangeRejected =
            new("Application.CannotChangeRejected", "You cannot work with a rejected application.");

    public static readonly Error CannotChangeApprove =
        new("Application.CannotChangeApprove", "You cannot approve an already rejected application.");

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

    public static readonly Error InterviewDateTooSoon =
        new("Application.InterviewDateTooSoon", "The interview should take place no earlier than 1 hour from the current moment.");

    public static readonly Error InterviewDateInPast =
        new("Application.InterviewDateInPast", "The date of the interview cannot be in the past.");

    public static readonly Error RejectionReasonTooShort =
        new("Application.RejectionReasonTooShort", "The reason for rejection must be no shorter than 15 characters.");

    public static readonly Error RejectionReasonInvalid =
        new("Application.RejectionReasonInvalid", "The reason for the rejection must contain letters and be clear.");

    public static readonly Error InterviewSlotConflict =
        new("Application.InterviewSlotConflict", "This administrator already has an interview at the selected time. Choose a different time.");
}