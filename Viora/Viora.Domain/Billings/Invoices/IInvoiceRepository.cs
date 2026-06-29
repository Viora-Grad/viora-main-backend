namespace Viora.Domain.Billings.Invoices;

public interface IInvoiceRepository
{
    void Add(Invoice invoice);
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Invoice>> GetAllByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);

    // Next monotonic invoice sequence (drives the display number INV-yyyy-NNNNNN).
    Task<long> NextSequenceAsync(CancellationToken cancellationToken = default);
}
