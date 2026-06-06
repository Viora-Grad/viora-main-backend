
namespace Viora.Domain.Shared;

public record Currency
{
    public static readonly Currency Usd = new("USD");
    public static readonly Currency Egp = new("EGP");
    public string Code { get; init; }
    private Currency(string code)
    {
        Code = code;
    }
    public static Currency FromCode(string code)
    {
        return SupportedCurrencies().FirstOrDefault(c => c.Code.Equals(code, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Unsupported currency code: {code}");
    }

    public static IReadOnlyCollection<Currency> SupportedCurrencies() => [Usd, Egp];
}