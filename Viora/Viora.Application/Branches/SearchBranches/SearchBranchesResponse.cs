using Viora.Domain.Branches.Internals;

namespace Viora.Application.Branches.SearchBranches;

public sealed record SearchBranchesResponse(
    Guid BranchId,
    Guid OrganizationId,
    string OrganizationName,
    bool IsOpen,
    DateTime OpenedSince,
    float Rating,
    BranchStatus Status,
    string Address,
    string CoverImageEncoded,   // represnets the first iamge added in the branch gallery
    string TimeLineId,
    Coordination Coordinations
    );

public sealed record Coordination(double Latitude, double Longitude);