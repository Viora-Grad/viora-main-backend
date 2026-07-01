using Microsoft.EntityFrameworkCore;
using Viora.Domain.Services;

namespace Viora.Infrastructure.Repositories;

internal class ServiceRepository : Repository<Service>, IServiceRepository
{
    public ServiceRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Service>> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<Service>()
            .AsNoTracking()
            .Where(service => service.BranchId == branchId)
            .OrderBy(service => service.Name.Value)
            .ToListAsync(cancellationToken);
    }
}
