using Viora.Domain.Abstractions;

namespace Viora.Domain.Feedbacks;

public static class FeedbackErrors
{
    public static readonly Error RatingRangeInvalid = new("Feedbacks.RatingRangeInvalid", "Rating must be within 1 and 10", ErrorCategory.Validation);
}
