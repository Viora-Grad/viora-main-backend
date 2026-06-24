using Viora.Domain.Abstractions;
using Viora.Domain.Billings.Invoices;
using Viora.Domain.Shared;

namespace Viora.Domain.Billings.Invoices.Internals;

public sealed record InvoiceItem
{
    private InvoiceItem() { }
    private InvoiceItem(int number, string name, string description, int quantity, Money price, decimal discountPercentage, decimal taxPercentage)
    {
        _number = number;
        ItemName = name;
        Description = description;
        Quantity = quantity;
        Price = price;
        DiscountPercentage = discountPercentage;
        TaxPercentage = taxPercentage;
    }

    private readonly int _number;
    private const string _prefix = "ITM-";

    public string ItemNumber => _prefix + _number;
    public string ItemName { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public int Quantity { get; private set; }

    public Money Price { get; private set; } = default!;

    public decimal DiscountPercentage { get; private set; }
    public decimal TaxPercentage { get; private set; }

    public Money SubTotal => new(Quantity * Price.Amount, Price.Currency);
    public Money DiscountAmount => new(SubTotal.Amount * DiscountPercentage, Price.Currency);
    public Money NetAmount => SubTotal - DiscountAmount;
    public Money TaxAmount => new(NetAmount.Amount * TaxPercentage, Price.Currency);
    public Money TotalAmount => NetAmount + TaxAmount;

    public static Result<IEnumerable<InvoiceItem>> Create(IEnumerable<InvoiceItemHolder> items, decimal taxPercentage)
    {
        var holders = items.ToList();

        if (holders.Count == 0)
            return Result.Failure<IEnumerable<InvoiceItem>>(InvoiceErrors.InvoiceItemsEmpty);

        if (taxPercentage is < 0 or > 1)
            return Result.Failure<IEnumerable<InvoiceItem>>(InvoiceErrors.InvalidTaxPercentage);

        var result = new List<InvoiceItem>(holders.Count);
        var counter = 1;
        foreach (var item in holders)
        {
            if (item.Price.Amount <= 0)
                return Result.Failure<IEnumerable<InvoiceItem>>(InvoiceErrors.InvoiceItemZeroOrLess);

            if (item.Quantity <= 0)
                return Result.Failure<IEnumerable<InvoiceItem>>(InvoiceErrors.InvoiceItemQuantityZeroOrLess);

            if (item.DiscountPercentage is < 0 or > 1)
                return Result.Failure<IEnumerable<InvoiceItem>>(InvoiceErrors.InvalidDiscountPercentage);

            result.Add(new InvoiceItem(counter, item.Name, item.Description, item.Quantity, item.Price, item.DiscountPercentage, taxPercentage));
            counter++;
        }

        return Result.Success<IEnumerable<InvoiceItem>>(result);
    }

}

public sealed record InvoiceItemHolder(string Name, string Description, int Quantity, Money Price, decimal DiscountPercentage);