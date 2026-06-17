using Microsoft.EntityFrameworkCore;
using Viora.Domain.Feedbacks;

namespace Viora.Infrastructure.Repositories;

internal sealed class FeedbackRepository(ApplicationDbContext dbContext) : IFeedbackRepository
{
    public Task<Dictionary<Guid, double>> GetAverageRatingsByBranchIdsAsync(
        IEnumerable<Guid> branchIds,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Feedback>()
            .Where(f => branchIds.Contains(f.BranchId))
            .GroupBy(f => f.BranchId)
            .Select(g => new
            {
                BranchId = g.Key,
                Avg = g.Average(f =>
                    (double)(f.Ratings.ServiceOutOfTen + f.Ratings.BranchOutOfTen + f.Ratings.SystemOutOfTen) / 3)
            })
            .ToDictionaryAsync(x => x.BranchId, x => x.Avg, cancellationToken);
    }
}
