using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;

namespace Viora.Application.Archives.GetFolderTree;

public sealed record GetFolderTreeQuery(Guid ArchiveId) : IQuery<ArchiveTreeNode>;
