using Viora.Domain.Abstractions;

namespace Viora.Domain.Branches.Internals;

public record BusinessHour
{
    public DayOfWeek Day { get; init; }
    public TimeSpan OpenTime { get; init; }
    public TimeSpan CloseTime { get; init; }

    private BusinessHour(DayOfWeek day, TimeSpan openTime, TimeSpan closeTime)
    {
        Day = day;
        OpenTime = openTime;
        CloseTime = closeTime;
    }
    public static Result<BusinessHour> Create(DayOfWeek day, TimeSpan openTime, TimeSpan closeTime)
    {
        if (openTime < TimeSpan.Zero || openTime > TimeSpan.FromDays(1))
            return Result.Failure<BusinessHour>(BranchErrors.InvalidOpenTimeInterval);

        if (closeTime < TimeSpan.Zero || closeTime > TimeSpan.FromDays(1))
            return Result.Failure<BusinessHour>(BranchErrors.InvalidCloseTimeInterval);

        if (closeTime <= openTime)
            return Result.Failure<BusinessHour>(BranchErrors.OpenTimeAfterCloseTime);

        return Result.Success(new BusinessHour(day, openTime, closeTime));
    }
}
