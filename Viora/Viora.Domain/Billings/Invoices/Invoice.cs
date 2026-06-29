using Viora.Domain.Abstractions;
using Viora.Domain.Billings.Invoices.Internals;
using Viora.Domain.Shared;
using Viora.Domain.Shared.Internal;

namespace Viora.Domain.Billings.Invoices;

public sealed class Invoice : Entity
{
    public Guid OrganizationId { get; private set; }
    public OrganizationName OrganizationName { get; private set; } = default!;
    public Email BillTo { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? DueDateUtc { get; private set; }
    public decimal TaxPercentage { get; private set; }
    public Currency Currency { get; private set; } = default!;
    public InvoiceStatus Status { get; private set; }
    private long Sequence { get; set; }
    public Number Number => $"INV-{CreatedAtUtc:yyyy}-{Sequence:D6}";

    public ExternalPayment? ExternalPayment { get; set; }

    public IReadOnlyList<InvoiceItem> Items => _items.AsReadOnly();
    private readonly List<InvoiceItem> _items = [];

    public Money SubTotal => Sum(item => item.SubTotal);
    public Money TotalDiscount => Sum(item => item.DiscountAmount);
    public Money NetTotal => Sum(item => item.NetAmount);
    public Money TotalTax => Sum(item => item.TaxAmount);
    public Money Total => Sum(item => item.TotalAmount);

    private Invoice() { }

    public static Result<Invoice> Create(
        Guid organizationId,
        string organizationName,
        string billTo,
        long sequence,
        DateTime currentDateTime,
        decimal taxPercentage,
        IEnumerable<InvoiceItemHolder> items)
    {
        if (string.IsNullOrWhiteSpace(organizationName))
            return Result.Failure<Invoice>(InvoiceErrors.EmptyOrganizationName);

        if (string.IsNullOrWhiteSpace(billTo) || !billTo.Contains('@'))
            return Result.Failure<Invoice>(InvoiceErrors.InvalidBillTo);

        var holders = items.ToList();

        if (holders.Select(holder => holder.Price.Currency).Distinct().Count() > 1)
            return Result.Failure<Invoice>(InvoiceErrors.MixedCurrencies);

        var itemsResult = InvoiceItem.Create(holders, taxPercentage);
        if (itemsResult.IsFailure)
            return Result.Failure<Invoice>(itemsResult.Error);

        var invoiceItems = itemsResult.Value.ToList();

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            OrganizationName = organizationName,
            BillTo = new Email(billTo),
            Sequence = sequence,
            CreatedAtUtc = currentDateTime,
            TaxPercentage = taxPercentage,
            Currency = invoiceItems[0].Price.Currency,
            Status = InvoiceStatus.Draft,
        };
        invoice._items.AddRange(invoiceItems);

        return Result.Success(invoice);
    }

    public Result Issue(DateTime dueDateUtc)
    {
        if (Status != InvoiceStatus.Draft)
            return Result.Failure(InvoiceErrors.OnlyDraftCanBeIssued);

        if (dueDateUtc < CreatedAtUtc)
            return Result.Failure(InvoiceErrors.DueDateBeforeCreation);

        Status = InvoiceStatus.Issued;
        DueDateUtc = dueDateUtc;
        return Result.Success();
    }

    public Result MarkPaid()
    {
        if (Status is not (InvoiceStatus.Issued or InvoiceStatus.Overdue))
            return Result.Failure(InvoiceErrors.OnlyIssuedCanBePaid);

        Status = InvoiceStatus.Paid;
        return Result.Success();
    }

    public Result Void()
    {
        if (Status == InvoiceStatus.Paid)
            return Result.Failure(InvoiceErrors.PaidCannotBeVoided);

        if (Status == InvoiceStatus.Void)
            return Result.Failure(InvoiceErrors.AlreadyVoid);

        Status = InvoiceStatus.Void;
        return Result.Success();
    }

    public Result MarkOverdue(DateTime currentDateTime)
    {
        if (Status != InvoiceStatus.Issued)
            return Result.Failure(InvoiceErrors.OnlyIssuedCanBecomeOverdue);

        if (DueDateUtc is null || currentDateTime <= DueDateUtc)
            return Result.Failure(InvoiceErrors.NotPastDue);

        Status = InvoiceStatus.Overdue;
        return Result.Success();
    }

    private Money Sum(Func<InvoiceItem, Money> selector) =>
        _items.Aggregate(Money.Zero(Currency), (running, item) => running + selector(item));
}
