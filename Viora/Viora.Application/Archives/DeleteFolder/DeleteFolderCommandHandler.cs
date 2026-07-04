using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;

namespace Viora.Application.Archives.DeleteFolder;

internal class DeleteFolderCommandHandler(
    IFolderRepository folderRepository) : ICommandHandler<DeleteFolderCommand>
{
    public async Task<Result> Handle(DeleteFolderCommand request, CancellationToken cancellationToken)
    {
        var folder = await folderRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Folder with id {request.Id} not found");

        folderRepository.Remove(folder);
        return Result.Success();
    }
}
