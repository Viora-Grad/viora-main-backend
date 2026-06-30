using Microsoft.EntityFrameworkCore;
using Viora.Domain.MedicalRecords;


namespace Viora.Infrastructure.Repositories.MedicalRecords;

internal class MedicalRecordRepository : Repository<MedicalRecord>, IMedicalRecordRepository
{
    public MedicalRecordRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<MedicalRecord?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken)
    {
        return await DbContext.Set<MedicalRecord>()
            .FirstOrDefaultAsync(mr => mr.CustomerId == customerId, cancellationToken);
    }
}
