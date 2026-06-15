using FluentValidation;
using Viora.Domain.Branches.Internals;

namespace Viora.Application.Branches.UpdateSchedule;

internal class UpdateScheduleCommandValidator : AbstractValidator<UpdateScheduleCommand>
{
    public UpdateScheduleCommandValidator()
    {
        RuleFor(x => x.Schedule)
            .NotEmpty().WithMessage("Schedule cannot be empty.")
            .Must(schedule => schedule != null && schedule.Count() <= 7)
            .WithMessage("Schedule cannot contain more than 7 days.")
            .Must(HaveNoDuplicateDays)
            .WithMessage("Schedule contains duplicate days of the week.");
    }

    private bool HaveNoDuplicateDays(IEnumerable<BusinessHour> schedule)
    {
        if (schedule == null) return true;

        var seenDays = new HashSet<DayOfWeek>();
        foreach (var item in schedule)
        {
            if (item != null && !seenDays.Add(item.Day))
            {
                return false; // Found a duplicate day immediately
            }
        }
        return true;
    }
}
