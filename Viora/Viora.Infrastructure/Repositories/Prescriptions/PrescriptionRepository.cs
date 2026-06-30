using Microsoft.EntityFrameworkCore;
using Viora.Domain.Prescriptions;

namespace Viora.Infrastructure.Repositories.Prescriptions;

internal class PrescriptionRepository : Repository<Prescription>, IPrescriptionRepository
{
    public PrescriptionRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Prescription?> GetByAppointmentIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return DbContext.Set<Prescription>()
            .Include(p => p.items)
            .FirstOrDefaultAsync(p => p.AppointmentId == id);
    }

    public override Task<Prescription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return DbContext.Set<Prescription>()
            .Include(p => p.items)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
