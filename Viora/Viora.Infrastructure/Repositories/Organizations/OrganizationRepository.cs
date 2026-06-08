using Microsoft.EntityFrameworkCore;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Infrastructure.Presistance;

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

    public async Task<IReadOnlyList<Organization>> ListAsync(
        ISpecification<Organization> spec,
        CancellationToken cancellationToken = default)
    {
        return await SpecificationEvaluator<Organization>
            .GetQuery(DbContext.Set<Organization>().AsQueryable(), spec)
            .ToListAsync(cancellationToken);
    }
}
