using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;

namespace Viora.Application.Archives.CreateArchive;

public sealed record CreateArchiveCommand(
    Guid OrganizationId,
    string Name,
    string Description,
    bool EnableVersioning,
    bool EnableAttachments,
    bool EnableExport,
    bool EnableAudit
) : ICommand<ArchiveResponse>;
