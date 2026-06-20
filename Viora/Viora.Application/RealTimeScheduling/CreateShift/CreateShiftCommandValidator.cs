using FluentValidation;

namespace Viora.Application.RealTimeScheduling.CreateRecurringSchedule;

public class CreateShiftCommandValidator : AbstractValidator<CreateShiftCommand>
{
    public CreateShiftCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .NotEmpty()
            .WithMessage("BranchId is required.");

        RuleFor(x => x.StartTime)
            .LessThan(x => x.EndTime)
            .WithMessage("StartTime must be less than EndTime.");

        RuleFor(x => x.DayOfWeek)
            .NotEmpty()
            .WithMessage("DayOfWeek is required.")
            .Must(day => Enum.TryParse<DayOfWeek>(day, true, out _))
            .WithMessage("DayOfWeek must be a valid day of the week.");


        RuleFor(x => x.StaffId)
            .NotEmpty()
            .WithMessage("StaffId is required.");
    }
}
