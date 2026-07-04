namespace Viora.Application.Archives.Shared;

public sealed record TemplateResponse(
    Guid Id,
    Guid ArchiveId,
    Guid FolderId,
    string Name,
    string Description,
    int CurrentVersion,
    DateTime CreatedAt);
