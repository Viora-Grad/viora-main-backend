using Viora.Domain.Abstractions;
using Viora.Domain.Feedbacks.Events;
using Viora.Domain.Feedbacks.Internals;

namespace Viora.Domain.Feedbacks;

public sealed class Feedback : Entity
{
    public Guid BranchId { get; private set; }
    public Guid UserId { get; private set; }
    public Ratings Ratings { get; private set; } = default!;
    public Comment? Comment { get; private set; }
    public DateTime SubmittedOnUtc { get; private set; }
    public DateTime? EditedOnUtc { get; private set; }

    private Feedback() { }

    public static Result<Feedback> Create(Guid branchId, Guid userId, int serviceOutOfTen, int branchOutOfTen, int systemOutOfTen, DateTime currentDateTime, string? comment = null)
    {
        var ratingsResult = Ratings.Create(serviceOutOfTen, branchOutOfTen, systemOutOfTen);
        if (ratingsResult.IsFailure)
            return Result.Failure<Feedback>(ratingsResult.Error);

        var result = new Feedback
        {
            Id = Guid.NewGuid(),
            BranchId = branchId,
            UserId = userId,
            Ratings = ratingsResult.Value,
            Comment = comment is null ? null : new Comment(comment),
            SubmittedOnUtc = currentDateTime
        };
        result.RaiseDomainEvent(new FeedbackSubmittedEvent(branchId, (serviceOutOfTen + branchOutOfTen + systemOutOfTen) / 3));

        return Result.Success(result);
    }

    public Result Update(int serviceOutOfTen, int branchOutOfTen, int systemOutOfTen, DateTime currentDateTime, string? comment = null)
    {
        var ratingsResult = Ratings.Create(serviceOutOfTen, branchOutOfTen, systemOutOfTen);
        if (ratingsResult.IsFailure)
            return Result.Failure(ratingsResult.Error);

        Ratings = ratingsResult.Value;
        EditedOnUtc = currentDateTime;
        Comment = comment is null ? null : new Comment(comment);

        RaiseDomainEvent(new FeedbackUpdatedEvent(BranchId, (serviceOutOfTen + branchOutOfTen + systemOutOfTen) / 3));

        return Result.Success();
    }

}
