using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Plans.Features;

namespace Viora.Application.Marketing.FinalizePost;

// Creates the archived Facebook post from the chat's latest content. Implements ILimitedFeatureCommand so
// the quota pipeline checks + consumes ONE marketing-post unit before the handler runs; the handler commits
// that decrement only by calling SaveChanges on success. On any failure it returns without saving, so the
// pipeline's in-memory decrement is discarded (quota preserved). OrganizationId is supplied by the caller
// (the orchestrator), which resolved it from the authenticated context.
public sealed record FinalizePostCommand(Guid ChatId, Guid OrganizationId) : ILimitedFeatureCommand<FinalizePostResult>
{
    public Guid LimitedFeatureId { get; init; } = LimitedFeature.MarketingAiPosts.Id;
    public long DeltaChange { get; init; } = -1;
}
