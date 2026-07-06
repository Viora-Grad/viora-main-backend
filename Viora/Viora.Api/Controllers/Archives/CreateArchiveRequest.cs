namespace Viora.Api.Controllers.Archives;

public sealed record CreateArchiveRequest(
    Guid OrganizationId,
    string Name,
    string? Description,
    bool EnableVersioning,
    bool EnableAttachments,
    bool EnableExport,
    bool EnableAudit
);
