using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;

namespace Viora.Application.Archives.GetFolderTree;

internal class GetFolderTreeQueryHandler(
    IArchiveRepository archiveRepository,
    IFolderRepository folderRepository,
    ITemplateRepository templateRepository) : IQueryHandler<GetFolderTreeQuery, ArchiveTreeNode>
{
    public async Task<Result<ArchiveTreeNode>> Handle(GetFolderTreeQuery request, CancellationToken cancellationToken)
    {
        var archive = await archiveRepository.GetByIdAsync(request.ArchiveId, cancellationToken)
            ?? throw new NotFoundException($"Archive with id {request.ArchiveId} not found");

        var foldersTask = folderRepository.GetByArchiveIdAsync(request.ArchiveId, cancellationToken);
        var templatesTask = templateRepository.GetByArchiveIdAsync(request.ArchiveId, cancellationToken);

        await Task.WhenAll(foldersTask, templatesTask);

        var folders = foldersTask.Result;
        var templates = templatesTask.Result;

        var templatesByFolder = templates
            .GroupBy(t => t.FolderId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var rootNode = new ArchiveTreeNode(
            archive.Id,
            archive.Name.Value,
            "Archive",
            0,
            BuildChildren(folders, templatesByFolder, null));

        return Result.Success(rootNode);
    }

    private static List<ArchiveTreeNode> BuildChildren(
        List<Folder> folders,
        Dictionary<Guid, List<Template>> templatesByFolder,
        Guid? parentId)
    {
        return folders
            .Where(f => f.ParentFolderId == parentId)
            .OrderBy(f => f.Order)
            .Select(f =>
            {
                var childFolders = BuildChildren(folders, templatesByFolder, f.Id);

                var templateNodes = templatesByFolder.TryGetValue(f.Id, out var templates)
                    ? templates
                        .OrderBy(t => t.Name.Value)
                        .Select(t => new ArchiveTreeNode(
                            t.Id,
                            t.Name.Value,
                            "Template",
                            0,
                            []))
                        .ToList()
                    : [];

                var children = childFolders.Concat(templateNodes).ToList();

                return new ArchiveTreeNode(
                    f.Id,
                    f.Name.Value,
                    "Folder",
                    f.Order,
                    children);
            })
            .ToList();
    }
}
