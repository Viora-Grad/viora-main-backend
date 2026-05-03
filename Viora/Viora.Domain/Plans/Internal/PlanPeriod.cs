using Viora.Domain.Abstractions;

namespace Viora.Domain.Plans.Internal;

public record PlanPeriod
{
    public static readonly PlanPeriod monthly = new(1, "Monthly");
    public static readonly PlanPeriod annually = new(2, "Annually");
    public static readonly PlanPeriod semiAnnually = new(3, "Semi-Annually");

    private PlanPeriod(int id, string name)
    {
        this.Id = id;
        this.Name = name;
    }
    public int Id { get; private set; }
    public string Name { get; private set; }

    public Result<DateTime> CalculateEndTime(DateTime periodStart)
    {
        if (Id == monthly.Id)
        {
            return Result.Success(periodStart.AddMonths(1));
        }
        else if (Id == annually.Id)
        {
            return Result.Success(periodStart.AddYears(1));
        }
        else if (Id == semiAnnually.Id)
        {
            return Result.Success(periodStart.AddMonths(6));
        }
        else
            return Result.Failure<DateTime>(PlanError.InvalidPlanPeriod);
    }

    public static PlanPeriod FromId(int id)
    {
        return id switch
        {
            1 => monthly,
            2 => annually,
            3 => semiAnnually,
            _ => throw new ArgumentException("Invalid PlanPeriod Id")
        };
    }
}

