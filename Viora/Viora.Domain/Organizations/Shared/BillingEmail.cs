namespace Viora.Domain.Organizations.Shared;

public record BillingEmail(string Value)
{
    public static implicit operator BillingEmail(string value) => new(value);
    public static implicit operator string(BillingEmail billingEmail) => billingEmail.Value;
}
