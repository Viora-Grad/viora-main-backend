using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;
using Viora.Domain.Archives.Internals;

namespace Viora.Application.Archives.UpdateFolder;

internal class UpdateFolderCommandHandler(
    IFolderRepository folderRepository) : ICommandHandler<UpdateFolderCommand>
{
    public async Task<Result> Handle(UpdateFolderCommand request, CancellationToken cancellationToken)
    {
        var folder = await folderRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Folder with id {request.Id} not found");

        folder.Update(
            new FolderName(request.Name),
            new FolderDescription(request.Description),
            request.Order);

        folderRepository.Update(folder);
        return Result.Success();
    }
}
