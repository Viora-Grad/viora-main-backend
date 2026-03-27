
namespace Viora.Domain.Shared;

public record Money(decimal Amount, Currency Currency)
{
    public static Money Zero(Currency currency)
    {
        return new Money(0m, currency);
    }
    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies.");
        return new Money(a.Amount + b.Amount, a.Currency);
    }
    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies.");
        return new Money(a.Amount - b.Amount, a.Currency);
    }
};