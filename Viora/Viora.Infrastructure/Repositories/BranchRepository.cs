using Viora.Domain.Branches;

namespace Viora.Infrastructure.Repositories;

internal class BranchRepository(ApplicationDbContext dbContext) : Repository<Branch>(dbContext), IBranchRepository
{
}
