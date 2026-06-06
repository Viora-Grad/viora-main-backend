using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Plans.Features;

namespace Viora.Application.Branches.AddBranch;

public sealed record AddBranchCommand(
    Guid OrganizationId,
    int AddressNumber,
    string AddressStreet,
    string AddressCity,
    string AddressState,
    Guid AddressCountryId,
    int AddressPostalCode,
    double Latitude,
    double Longitude,
    string ContactEmail,
    ICollection<string> ServicesProvided,
    string TimeZoneId) : ILimitedFeatureCommand<Guid>
{
    public Guid LimitedFeatureId { get; init; } = LimitedFeature.Branches.Id;
    public int DeltaChange { get; init; } = -1;
}
