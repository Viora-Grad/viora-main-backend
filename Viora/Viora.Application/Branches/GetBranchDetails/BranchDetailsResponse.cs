using Viora.Application.Abstractions.Media;
using Viora.Application.Branches.SharedResponses;
using Viora.Domain.Branches.Internals;

namespace Viora.Application.Branches.GetBranchDetails;

public sealed record BranchDetailsResponse(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    IReadOnlyCollection<string> Services,
    string Address,
    Coordinations Location,
    BranchStatus BranchStatus,
    string ContaceEmail,
    IReadOnlyCollection<string> PhoneNumbers,
    IReadOnlyCollection<BusinessHour> Schedule,
    string TimeZone,
    DateTime OpenedSinceUtc,
    IReadOnlyCollection<MediaResponse> Gallery,
    bool IsCurrentlyOpen
    );