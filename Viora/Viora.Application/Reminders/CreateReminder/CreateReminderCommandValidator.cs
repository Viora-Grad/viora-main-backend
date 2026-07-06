using FluentValidation;
using Viora.Application.Abstractions.Clock;

namespace Viora.Application.Reminders.CreateReminder;

internal class CreateReminderCommandValidator : AbstractValidator<CreateReminderCommand>
{
    private readonly IDateTimeProvider _clock;
    public CreateReminderCommandValidator(IDateTimeProvider clock)
    {
        _clock = clock;
        RuleFor(x => x.AppointmentId)
            .NotEmpty().WithMessage("AppointmentId is required");
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters");
        RuleFor(x => x.Body)
            .MaximumLength(500).WithMessage("Body must not exceed 500 characters");
        RuleFor(x => x.ScheduledFor)
            .GreaterThan(_clock.UtcNow).WithMessage("Scheduled time must be in the future");
    }
}
