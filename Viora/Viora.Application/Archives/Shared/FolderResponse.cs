namespace Viora.Application.Archives.Shared;

public sealed record FolderResponse(
    Guid Id,
    Guid ArchiveId,
    Guid? ParentFolderId,
    string Name,
    string Description,
    string Type,
    int Order,
    DateTime CreatedAt);
