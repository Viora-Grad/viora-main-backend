namespace Viora.Domain.Archives.Internals;

public sealed record ArchiveSettings
(
    bool EnableVersioning,
    bool EnableAttachments,
    bool EnableExport,
    bool EnableAudit
);