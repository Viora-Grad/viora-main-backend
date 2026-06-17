using Viora.Application.Abstractions.Media;
using Viora.Application.Branches.SharedResponses;
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
    MediaResponse? CoverImageEncoded,
    string TimeLineId,
    Coordinations Coordinations
    );