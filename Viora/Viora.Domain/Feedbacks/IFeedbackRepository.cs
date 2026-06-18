namespace Viora.Domain.Feedbacks;

public interface IFeedbackRepository
{
    /// <summary>
    /// Returns the average overall rating per branch for the given branch IDs.
    /// Only branches that have at least one feedback entry appear in the result.
    /// </summary>
    Task<Dictionary<Guid, double>> GetAverageRatingsByBranchIdsAsync(
        IEnumerable<Guid> branchIds,
        CancellationToken cancellationToken = default);
}
