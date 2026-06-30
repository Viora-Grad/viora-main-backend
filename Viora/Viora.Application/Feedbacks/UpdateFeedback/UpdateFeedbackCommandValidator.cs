using FluentValidation;

namespace Viora.Application.Feedbacks.UpdateFeedback;

internal class UpdateFeedbackCommandValidator : AbstractValidator<UpdateFeedbackCommand>
{
    public UpdateFeedbackCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User Id can not be empty");

        RuleFor(x => x.FeedbackId)
            .NotEmpty()
            .WithMessage("Feedback Id can not be empty");

        RuleFor(x => x.Comment)
            .Length(1, 1000);

        RuleFor(x => x.ServiceRatingOutOfTen)
            .InclusiveBetween(0, 10)
            .WithMessage("ServiceRatingOutOfTen must be between 0 and 10");

        RuleFor(x => x.BranchOutOfTen)
            .InclusiveBetween(0, 10)
            .WithMessage("BranchOutOfTen must be between 0 and 10");

        RuleFor(x => x.SystemExperienceOutOfTen)
            .InclusiveBetween(0, 10)
            .WithMessage("SystemExperienceOutOfTen must be between 0 and 10");
    }
}