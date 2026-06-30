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
            .Include("_gallery")
            .FirstOrDefaultAsync(cancellation);
    }

    public async Task<IReadOnlyList<Branch>> GetByOrganizationIdAsync(Guid orgId, CancellationToken cancellationToken = default)
    {
        var branches = await DbContext.Set<Branch>()
            .Where(x => x.OrganizationId == orgId)
            .OrderBy(x => x.OpenedAtUtc)
            .Include("_gallery")
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return branches.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<MediaFile>?> GetMediaByBranchId(Guid id, CancellationToken cancellation = default)
    {
        var branch = await DbContext.Set<Branch>()
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Include("_gallery")
            .FirstOrDefaultAsync(cancellation);

        return branch?.Gallery;
    }
    public void Attach(Branch branch)
    {
        DbContext.Set<Branch>().Attach(branch);
    }
}