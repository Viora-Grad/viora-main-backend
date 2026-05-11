namespace Viora.Domain.Vivi.Shared.Internals;

public sealed record UsageDetails(int? InputTokens, int? OutputTokens, int? LatencyMs);