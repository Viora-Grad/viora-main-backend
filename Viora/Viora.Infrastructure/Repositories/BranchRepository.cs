using Microsoft.EntityFrameworkCore;
using Viora.Domain.Branches;
using Viora.Domain.Medias;

namespace Viora.Infrastructure.Repositories;

internal class BranchRepository(ApplicationDbContext dbContext) : Repository<Branch>(dbContext), IBranchRepository
{
    public override async Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellation)
    {
        return await DbContext.Set<Branch>()
            .Where(r => r.Id == id)
            .Include(b => b.Gallery)
            .Include(b => b.PhoneNumbers)
            .FirstOrDefaultAsync(cancellation);
    }

    public async Task<IReadOnlyList<Branch>> GetByOrganizationIdAsync(Guid orgId, CancellationToken cancellationToken = default)
    {
        var branches = await DbContext.Set<Branch>()
            .Where(x => x.OrganizationId == orgId)
            .OrderBy(x => x.OpenedAtUtc)
            .Include(x => x.Gallery)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return branches.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<MediaFile>?> GetMediaByBranchId(Guid id, CancellationToken cancellation = default)
    {
        var gallery = await DbContext.Set<Branch>()
            .AsNoTracking()
            .Include(x => x.Gallery)
            .Select(x => x.Gallery)
            .FirstOrDefaultAsync(cancellation);

        return gallery;
    }
}