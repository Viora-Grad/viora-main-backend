using Viora.Application.Abstractions.Messaging;

namespace Viora.Application.Archives.UpdateArchive;

public sealed record UpdateArchiveCommand(
    Guid Id,
    string Name,
    string Description,
    bool EnableVersioning,
    bool EnableAttachments,
    bool EnableExport,
    bool EnableAudit
) : ICommand;
