using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;
using Viora.Domain.Archives.Internals;

namespace Viora.Application.Archives.CreateFolder;

internal class CreateFolderCommandHandler(
    IFolderRepository folderRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateFolderCommand, FolderResponse>
{
    public async Task<Result<FolderResponse>> Handle(CreateFolderCommand request, CancellationToken cancellationToken)
    {
        var folderType = new FolderType(request.Type);

        var folder = Folder.Create(
            request.ArchiveId,
            request.ParentFolderId,
            new FolderName(request.Name),
            new FolderDescription(request.Description),
            folderType,
            request.Order,
            dateTimeProvider.UtcNow);

        folderRepository.Add(folder);

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
