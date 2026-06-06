namespace Viora.Domain.Vivi.Messages.Internals;

public sealed record UsageDetails(int? InputTokens, int? OutputTokens, int? LatencyMs);