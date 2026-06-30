namespace Viora.Domain.Feedbacks;

public interface IFeedbackRepository
{
    void Add(Feedback feedback);

    /// <summary>
    /// Returns the average overall rating per branch for the given branch IDs.
    /// Only branches that have at least one feedback entry appear in the result.
    /// </summary>
    Task<Dictionary<Guid, double>> GetAverageRatingsByBranchIdsAsync(
        IEnumerable<Guid> branchIds,
        CancellationToken cancellationToken = default);

    Task<Feedback?> GetByIdAsync(Guid Id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Feedback>> GetByUserIdAsync(Guid UserId, CancellationToken cancellationToken);
    Task<Feedback?> GetByUserIdAsync(Guid UserId, Guid BranchId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns a page of feedbacks (newest first) along with the total count, optionally
    /// filtered by branch and/or user. Null filters are ignored.
    /// </summary>
    Task<(IReadOnlyList<Feedback> Items, long TotalCount)> GetPagedAsync(
        Guid? branchId,
        Guid? userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
