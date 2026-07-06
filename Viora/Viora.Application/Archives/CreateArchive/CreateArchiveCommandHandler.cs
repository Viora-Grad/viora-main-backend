using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Archives.Shared;
using Viora.Domain.Abstractions;
using Viora.Domain.Archives;
using Viora.Domain.Archives.Internals;

namespace Viora.Application.Archives.CreateArchive;

internal class CreateArchiveCommandHandler(
    IArchiveRepository archiveRepository,
    IFolderRepository folderRepository,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateArchiveCommand, ArchiveResponse>
{
    public async Task<Result<ArchiveResponse>> Handle(CreateArchiveCommand request, CancellationToken cancellationToken)
    {
        var archive = Archive.Create(
            request.OrganizationId,
            new ArchiveName(request.Name),
            new ArchiveDescription(request.Description),
            new ArchiveSettings(
                request.EnableVersioning,
                request.EnableAttachments,
                request.EnableExport,
                request.EnableAudit),
            dateTimeProvider.UtcNow);

        var rootFolder = Folder.Create(
            archive.Id,
            null,
            new FolderName("Root"),
            new FolderDescription("Root folder"),
            FolderType.Root,
            0,
            dateTimeProvider.UtcNow);

        archiveRepository.Add(archive);
        folderRepository.Add(rootFolder);

        var response = new ArchiveResponse(
            archive.Id,
            archive.OrganizationId,
            archive.Name.Value,
            archive.Description.Value,
            archive.RootFolder,
            archive.Setting,
            archive.CreatedAt);

        return Result.Success(response);
    }
}
