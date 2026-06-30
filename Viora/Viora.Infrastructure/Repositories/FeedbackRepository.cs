using Microsoft.EntityFrameworkCore;
using Viora.Domain.Feedbacks;

namespace Viora.Infrastructure.Repositories;

internal sealed class FeedbackRepository(ApplicationDbContext dbContext) : Repository<Feedback>(dbContext), IFeedbackRepository
{
    public Task<Dictionary<Guid, double>> GetAverageRatingsByBranchIdsAsync(
        IEnumerable<Guid> branchIds,
        CancellationToken cancellationToken = default)
    {
        return DbContext.Set<Feedback>()
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

    public async Task<IReadOnlyCollection<Feedback>> GetByUserIdAsync(Guid UserId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Feedback>().Where(x => x.UserId == UserId).ToListAsync(cancellationToken);
    }

    public async Task<Feedback?> GetByUserIdAsync(Guid UserId, Guid BranchId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<Feedback>().Where(x => x.UserId == UserId && x.BranchId == BranchId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Feedback> Items, long TotalCount)> GetPagedAsync(
        Guid? branchId,
        Guid? userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbContext.Set<Feedback>().AsQueryable();

        if (branchId is not null)
            query = query.Where(f => f.BranchId == branchId.Value);

        if (userId is not null)
            query = query.Where(f => f.UserId == userId.Value);

        var totalCount = await query.LongCountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(f => f.SubmittedOnUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
