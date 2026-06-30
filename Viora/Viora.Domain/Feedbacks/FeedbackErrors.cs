using Viora.Domain.Abstractions;

namespace Viora.Domain.Feedbacks;

public static class FeedbackErrors
{
    public static readonly Error RatingRangeInvalid = new("Feedbacks.RatingRangeInvalid", "Rating must be within 1 and 10", ErrorCategory.Validation);
    public static readonly Error AlreadyRated = new("Feedbacks.AlreadyRated", "Update the rating instead of adding new one", ErrorCategory.Conflict);
    public static readonly Error UserNotOwnerOfFeedback = new("Feedbacks.UserNotOwnerOfFeedback", "User must own the feedback to edit", ErrorCategory.Unauthorized);
    public static readonly Error UserHasNoAppointmentsInBranch = new("Feedbacks.UserHasNoAppointmentsInBranch", "User must have had atleast one appointment", ErrorCategory.Conflict);
}
