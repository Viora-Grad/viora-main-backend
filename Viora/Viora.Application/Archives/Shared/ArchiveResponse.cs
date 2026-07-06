using Viora.Domain.Archives.Internals;

namespace Viora.Application.Archives.Shared;

public sealed record ArchiveResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string Description,
    Guid RootFolder,
    ArchiveSettings Settings,
    DateTime CreatedAt);
