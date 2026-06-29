using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Billings.GetOrganizationInvoices;

public sealed record GetOrganizationInvoicesQuery(Guid OrganizationId)
    : IQuery<IReadOnlyList<InvoiceResponse>>;
