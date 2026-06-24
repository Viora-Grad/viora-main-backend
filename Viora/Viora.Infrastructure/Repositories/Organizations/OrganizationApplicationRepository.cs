using Microsoft.EntityFrameworkCore;
using Viora.Domain.Organizations.OnBoardings;
using Viora.Domain.Organizations.OnBoardings.Internals;

namespace Viora.Infrastructure.Repositories.Organizations;

internal class OrganizationApplicationRepository(ApplicationDbContext dbContext) : Repository<OrganizationApplication>(dbContext), IOrganizationApplicationRepository
{
    public async Task<OrganizationApplication?> GetActiveApplicationByOrganizationNameAsync(string proposedName, CancellationToken cancellationToken)
    {
        return await DbContext.Set<OrganizationApplication>().FirstOrDefaultAsync(a => a.ProposedName == proposedName, cancellationToken: cancellationToken);
    }

    public async Task<OrganizationApplication?> GetLatestApplicationForOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<OrganizationApplication>()
            .AsNoTracking()
            .Where(a => a.OwnerId == ownerId)
            .OrderByDescending(a => a.SubmittedOnUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsApplicationSubmittedForOwnerAsync(Guid id, CancellationToken cancellation = default)
    {
        return await DbContext.Set<OrganizationApplication>().AnyAsync(a => a.OwnerId == id, cancellation);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<OrganizationApplication>()
            .AnyAsync(o => o.ProposedName == name && o.Status == ApplicationStatus.Pending);
    }
}
