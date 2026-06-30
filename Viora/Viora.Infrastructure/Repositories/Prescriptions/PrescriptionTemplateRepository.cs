using Microsoft.EntityFrameworkCore;
using Viora.Domain.Prescriptions;

namespace Viora.Infrastructure.Repositories.Prescriptions;

internal class PrescriptionTemplateRepository : Repository<PrescriptionTemplate>, IPrescriptionTemplateRepository
{
    public PrescriptionTemplateRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<List<PrescriptionTemplate>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        return DbContext.Set<PrescriptionTemplate>()
            .Include(pt => pt.File)
            .Where(pt => pt.OrganizationId == organizationId)
            .ToListAsync();
    }

    public override Task<PrescriptionTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return DbContext.Set<PrescriptionTemplate>()
            .Include(pt => pt.File)
            .FirstOrDefaultAsync(pt => pt.Id == id);
    }
}
