using FluentValidation;

namespace Viora.Application.RealTimeScheduling.CreateSchedule;

public class CreateScheduleValidator : AbstractValidator<CreateScheduleCommand>
{
    public CreateScheduleValidator()
    {

        RuleFor(x => x.BranchId)
             .NotEmpty()
             .WithMessage("BranchId is required.");

        RuleFor(x => x.DayOfWeek)
            .NotEmpty()
            .WithMessage("DayOfWeek is required.")
            .Must(day => Enum.TryParse<DayOfWeek>(day, true, out _))
            .WithMessage("DayOfWeek must be a valid day of the week.");

    }
}
