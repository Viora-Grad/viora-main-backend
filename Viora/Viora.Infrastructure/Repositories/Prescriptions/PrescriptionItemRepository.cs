using Viora.Domain.Prescriptions;

namespace Viora.Infrastructure.Repositories.Prescriptions;

internal class PrescriptionItemRepository : Repository<PrescriptionItem>, IPrescriptionItemRepository
{
    public PrescriptionItemRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
