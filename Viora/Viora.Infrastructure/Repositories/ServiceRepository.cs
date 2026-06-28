using Viora.Domain.Services;

namespace Viora.Infrastructure.Repositories;

internal class ServiceRepository : Repository<Service>, IServiceRepository
{
    public ServiceRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
