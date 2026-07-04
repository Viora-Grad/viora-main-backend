namespace Viora.Application.Archives.Shared;

public sealed record FolderTreeNode(
    Guid Id,
    Guid ArchiveId,
    Guid? ParentFolderId,
    string Name,
    string Description,
    string Type,
    int Order,
    List<FolderTreeNode> Children);
