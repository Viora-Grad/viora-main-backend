using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Plans.Features;

namespace Viora.Application.Branches.AddBranch;

public sealed record AddBranchCommand(Guid OrganizationId) : ILimitedFeatureCommand
{
    public Guid LimitedFeatureId { get; init; } = LimitedFeature.Branches.Id;
    public int DeltaChange { get; init; } = -1;
}
