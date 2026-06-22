using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Medias;
using Viora.Domain.Organizations.LegalPapers;
using Viora.Domain.Organizations.OnBoardings;

namespace Viora.Application.LegalPapers.GetLegalPaperFile;

internal sealed class GetLegalPaperFileQueryHandler(
    ILegalPaperRepository legalPaperRepository,
    IOrganizationApplicationRepository applicationRepository,
    IMediaRepository mediaRepository,
    IStorageService storageService) : IQueryHandler<GetLegalPaperFileQuery, LegalPaperFileResponse>
{
    public async Task<Result<LegalPaperFileResponse>> Handle(GetLegalPaperFileQuery request, CancellationToken cancellationToken)
    {
        var legalPaper = await legalPaperRepository.GetByIdAsync(request.LegalPaperId, cancellationToken);
        if (legalPaper is null)
            return Result.Failure<LegalPaperFileResponse>(LegalPaperErrors.NotFound);

        // Access is governed by the owning application, never by the media id. A privileged
        // reviewer may read any application's papers; otherwise the caller must own the
        // application the paper belongs to.
        if (!request.IsPrivileged)
        {
            var application = await applicationRepository.GetByIdAsync(legalPaper.ApplicationId, cancellationToken);
            if (application is null || application.OwnerId != request.RequesterId)
                return Result.Failure<LegalPaperFileResponse>(LegalPaperErrors.FileAccessDenied);
        }

        var media = await mediaRepository.GetByIdAsync(legalPaper.AttachmentId, cancellationToken);
        if (media is null)
            return Result.Failure<LegalPaperFileResponse>(LegalPaperErrors.FileMissing);

        var stream = storageService.GetFileStream(media.Key);
        return Result.Success(new LegalPaperFileResponse(stream, media.MimeType.Value, media.Name.Value));
    }
}
