using Microsoft.EntityFrameworkCore;
using Viora.Domain.Organizations.OrganizationDetails;

namespace Viora.Infrastructure.Repositories.Organizations;

internal sealed class OrganziationRepository(ApplicationDbContext context) : Repository<Organization>(context), IOrganizationRepository
{
    public async Task<Organization?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Organization>().FirstOrDefaultAsync(o => o.Name == name, cancellationToken);
    }

    public async Task<bool> IsOrganizationExistForOwnerAsync(
        Guid ownerId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Organization>()
            .AnyAsync(o => o.OwnerId == ownerId, cancellationToken);
    }
}
