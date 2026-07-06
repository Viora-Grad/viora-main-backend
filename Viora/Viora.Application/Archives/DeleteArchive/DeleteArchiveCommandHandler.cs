using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;

namespace Viora.Application.Archives.DeleteArchive;

internal class DeleteArchiveCommandHandler(
    IArchiveRepository archiveRepository) : ICommandHandler<DeleteArchiveCommand>
{
    public async Task<Result> Handle(DeleteArchiveCommand request, CancellationToken cancellationToken)
    {
        var archive = await archiveRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Archive with id {request.Id} not found");

        archiveRepository.Remove(archive);
        return Result.Success();
    }
}
