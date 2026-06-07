using Viora.Domain.Abstractions;

namespace Viora.Domain.Feedbacks.Internals;

public record Ratings
{
    public int ServiceOutOfTen { get; init; }
    public int BranchOutOfTen { get; init; }
    public int SystemOutOfTen { get; init; }

    public float Overall => (ServiceOutOfTen + BranchOutOfTen + SystemOutOfTen) / 3;

    private Ratings(int serviceOutOfTen, int branchOutOfTen, int systemOutOfTen)
    {
        ServiceOutOfTen = serviceOutOfTen;
        BranchOutOfTen = branchOutOfTen;
        SystemOutOfTen = systemOutOfTen;
    }

    public static Result<Ratings> Create(int serviceOutOfTen, int branchOutOfTen, int systemOutOfTen)
    {
        if (serviceOutOfTen > 10 || serviceOutOfTen <= 0
            || branchOutOfTen > 10 || branchOutOfTen <= 0
            || systemOutOfTen > 10 || systemOutOfTen <= 0)
            return Result.Failure<Ratings>(FeedbackErrors.RatingRangeInvalid);

        return Result.Success(new Ratings(serviceOutOfTen, branchOutOfTen, systemOutOfTen));
    }
}
