namespace Viora.Application.Archives.Shared;

public sealed record ArchiveTreeNode(
    Guid Id,
    string Name,
    string NodeType,
    int Order,
    List<ArchiveTreeNode> Children
);
