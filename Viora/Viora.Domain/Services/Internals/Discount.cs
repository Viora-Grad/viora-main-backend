namespace Viora.Domain.Services.Internals;

public sealed record Discount(int PercentageOutOf100, string Reason, DateTime StartDateUtc, DateTime EndDateUtc);