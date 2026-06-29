using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Billings.Invoices;
using Viora.Domain.Organizations.OrganizationDetails;

namespace Viora.Application.Billings.GetOrganizationInvoices;

internal sealed class GetOrganizationInvoicesQueryHandler(
    IInvoiceRepository invoiceRepository,
    IOrganizationRepository organizationRepository)
    : IQueryHandler<GetOrganizationInvoicesQuery, IReadOnlyList<InvoiceResponse>>
{
    public async Task<Result<IReadOnlyList<InvoiceResponse>>> Handle(
        GetOrganizationInvoicesQuery request, CancellationToken cancellationToken)
    {
        _ = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"the organization with id {request.OrganizationId} not found");

        var invoices = await invoiceRepository.GetAllByOrganizationIdAsync(request.OrganizationId, cancellationToken);

        IReadOnlyList<InvoiceResponse> result = invoices.Select(InvoiceResponse.Map).ToList();

        return Result.Success(result);
    }
}
