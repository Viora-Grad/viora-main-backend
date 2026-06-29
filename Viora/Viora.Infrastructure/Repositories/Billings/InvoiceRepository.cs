using Microsoft.EntityFrameworkCore;
using Viora.Domain.Billings.Invoices;

namespace Viora.Infrastructure.Repositories.Billings;

internal sealed class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    // v1: MAX(Sequence)+1. Sequence is a private member, read via EF.Property.
    // Note: Number has no unique index, so a race only yields a cosmetic duplicate display number.
    // Production should back this with a SQL SEQUENCE.
    public async Task<long> NextSequenceAsync(CancellationToken cancellationToken = default)
    {
        var hasAny = await DbContext.Set<Invoice>().AnyAsync(cancellationToken);
        if (!hasAny)
            return 1;

        var max = await DbContext.Set<Invoice>()
            .MaxAsync(invoice => EF.Property<long>(invoice, "Sequence"), cancellationToken);

        return max + 1;
    }
}
