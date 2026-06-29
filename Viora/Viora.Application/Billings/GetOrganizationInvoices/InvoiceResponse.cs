using Viora.Domain.Billings.Invoices;
using Viora.Domain.Billings.Invoices.Internals;

namespace Viora.Application.Billings.GetOrganizationInvoices;

public sealed record InvoiceResponse(
    Guid Id,
    string Number,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? DueDateUtc,
    string Currency,
    decimal SubTotal,
    decimal TotalTax,
    decimal Total,
    string? PaymentUrl,
    IReadOnlyList<InvoiceItemResponse> Items)
{
    public static InvoiceResponse Map(Invoice invoice) => new(
        invoice.Id,
        invoice.Number,
        invoice.Status.ToString(),
        invoice.CreatedAtUtc,
        invoice.DueDateUtc,
        invoice.Currency.Code,
        invoice.SubTotal.Amount,
        invoice.TotalTax.Amount,
        invoice.Total.Amount,
        invoice.ExternalPayment?.Url,
        invoice.Items.Select(InvoiceItemResponse.Map).ToList());
}

public sealed record InvoiceItemResponse(
    string ItemNumber,
    string Name,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal Total)
{
    public static InvoiceItemResponse Map(InvoiceItem item) => new(
        item.ItemNumber,
        item.ItemName,
        item.Description,
        item.Quantity,
        item.Price.Amount,
        item.TotalAmount.Amount);
}
