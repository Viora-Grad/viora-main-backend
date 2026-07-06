using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;
using Viora.Domain.Archives.Internals;

namespace Viora.Application.Archives.UpdateArchive;

internal class UpdateArchiveCommandHandler(
    IArchiveRepository archiveRepository) : ICommandHandler<UpdateArchiveCommand>
{
    public async Task<Result> Handle(UpdateArchiveCommand request, CancellationToken cancellationToken)
    {
        var archive = await archiveRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Archive with id {request.Id} not found");

        archive.Update(
            new ArchiveName(request.Name),
            new ArchiveDescription(request.Description),
            new ArchiveSettings(
                request.EnableVersioning,
                request.EnableAttachments,
                request.EnableExport,
                request.EnableAudit));

        archiveRepository.Update(archive);
        return Result.Success();
    }
}
