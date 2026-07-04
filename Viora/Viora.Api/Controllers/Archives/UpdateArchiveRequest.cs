namespace Viora.Api.Controllers.Archives;

public sealed record UpdateArchiveRequest(
    string Name,
    string? Description,
    bool EnableVersioning,
    bool EnableAttachments,
    bool EnableExport,
    bool EnableAudit
);
