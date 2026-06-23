using Viora.Domain.Shared;

namespace Viora.Application.Plans.Shared;

public class MoneyResponse
{
    public decimal amount { get; set; }
    public string currency { get; set; }


    public MoneyResponse(decimal amount, string currency)
    {
        this.amount = amount;
        this.currency = currency;
    }


    public static MoneyResponse MapToDTO(Money money)
    {
        return new MoneyResponse(money.Amount, money.Currency.Code);
    }
}
