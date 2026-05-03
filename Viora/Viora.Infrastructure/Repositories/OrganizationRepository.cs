using Viora.Domain.Organizations;

namespace Viora.Infrastructure.Repositories;

internal sealed class OrganziationRepository : Repository<Organization>, IOrganizationRepository
{
    public OrganziationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
