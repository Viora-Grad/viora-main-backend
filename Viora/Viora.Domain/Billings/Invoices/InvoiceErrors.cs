using Viora.Domain.Abstractions;

namespace Viora.Domain.Billings.Invoices;

public static class InvoiceErrors
{
    public static readonly Error InvoiceItemZeroOrLess = new("Invoices.InvoiceItemZeroOrLess", "Invoice Item price can not be zero or less", ErrorCategory.Validation);
    public static readonly Error InvoiceItemQuantityZeroOrLess = new("Invoices.InvoiceItemQuantityZeroOrLess", "Invoice Item Quantity can not be zero or less", ErrorCategory.Validation);
    public static readonly Error InvoiceItemsEmpty = new("Invoices.InvoiceItemsEmpty", "Items can not be empty", ErrorCategory.Validation);
    public static readonly Error InvalidDiscountPercentage = new("Invoices.InvalidDiscountPercentage", "Discount percentage must be a fraction between 0 and 1", ErrorCategory.Validation);
    public static readonly Error InvalidTaxPercentage = new("Invoices.InvalidTaxPercentage", "Tax percentage must be a fraction between 0 and 1", ErrorCategory.Validation);
    public static readonly Error EmptyOrganizationName = new("Invoices.EmptyOrganizationName", "Organization name is required", ErrorCategory.Validation);
    public static readonly Error InvalidBillTo = new("Invoices.InvalidBillTo", "A valid billing email is required", ErrorCategory.Validation);
    public static readonly Error MixedCurrencies = new("Invoices.MixedCurrencies", "All invoice items must share the same currency", ErrorCategory.Validation);
    public static readonly Error DueDateBeforeCreation = new("Invoices.DueDateBeforeCreation", "Due date cannot be before the invoice creation date", ErrorCategory.Validation);
    public static readonly Error OnlyDraftCanBeIssued = new("Invoices.OnlyDraftCanBeIssued", "Only a draft invoice can be issued", ErrorCategory.Conflict);
    public static readonly Error OnlyIssuedCanBePaid = new("Invoices.OnlyIssuedCanBePaid", "Only an issued or overdue invoice can be marked paid", ErrorCategory.Conflict);
    public static readonly Error PaidCannotBeVoided = new("Invoices.PaidCannotBeVoided", "A paid invoice cannot be voided", ErrorCategory.Conflict);
    public static readonly Error AlreadyVoid = new("Invoices.AlreadyVoid", "The invoice is already void", ErrorCategory.Conflict);
    public static readonly Error OnlyIssuedCanBecomeOverdue = new("Invoices.OnlyIssuedCanBecomeOverdue", "Only an issued invoice can become overdue", ErrorCategory.Conflict);
    public static readonly Error NotPastDue = new("Invoices.NotPastDue", "The invoice is not past its due date", ErrorCategory.Conflict);
}
