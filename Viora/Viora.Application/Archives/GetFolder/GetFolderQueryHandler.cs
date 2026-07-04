using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;

namespace Viora.Application.Archives.GetFolder;

internal class GetFolderQueryHandler(
    IFolderRepository folderRepository) : IQueryHandler<GetFolderQuery, FolderResponse>
{
    public async Task<Result<FolderResponse>> Handle(GetFolderQuery request, CancellationToken cancellationToken)
    {
        var folder = await folderRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Folder with id {request.Id} not found");

        var response = new FolderResponse(
            folder.Id,
            folder.ArchiveId,
            folder.ParentFolderId,
            folder.Name.Value,
            folder.Description.Value,
            folder.Type.Value,
            folder.Order,
            folder.CreatedAt);

        return Result.Success(response);
    }
}
