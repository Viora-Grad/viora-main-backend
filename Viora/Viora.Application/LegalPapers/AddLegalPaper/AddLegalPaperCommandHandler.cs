using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Scheduling;
using Viora.Domain.Abstractions;
using Viora.Domain.Medias;
using Viora.Domain.Organizations.LegalPapers;
using Viora.Domain.Organizations.LegalPapers.Events;
using Viora.Domain.Organizations.LegalPapers.Internals;
using Viora.Domain.Organizations.OnBoardings;

namespace Viora.Application.LegalPapers.AddLegalPaper;

internal sealed class AddLegalPaperCommandHandler(
    IOrganizationApplicationRepository applicationRepository,
    ILegalPaperRepository legalPapersRepository,
    IMediaRepository mediaRepository,
    IStorageService storageService,
    IStorageSettings storageSettings,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    IDomainEventScheduler scheduler) : ICommandHandler<AddLegalPaperCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddLegalPaperCommand request, CancellationToken cancellationToken)
    {
        var application = await applicationRepository.GetByIdAsync(request.ApplicationId, cancellationToken) ??
            throw new NotFoundException($"Application {request.ApplicationId} not found");

        var legalPapers = await legalPapersRepository.GetByApplicationIdAsync(request.ApplicationId, cancellationToken);

        var duplicate = legalPapers.FirstOrDefault(x => x.Type == request.Type);

        if (duplicate != null)
            return Result.Failure<Guid>(LegalPaperErrors.PaperExistsAndUpdated);

        if (request.UserId != application.OwnerId)
            throw new UnauthorizedAccessException("Only application owners can submit legal papers");

        var mediaContent = request.MediaContent;
        var extension = Path.GetExtension(mediaContent.FileName);
        var storageKey = $"legal-papers/{request.ApplicationId}/{Guid.NewGuid()}{extension}";
        var mediaResult = MediaFile.Create(
            mediaContent.FileName,
            mediaContent.SizeBytes,
            storageKey,
            mediaContent.ContentType,
            dateTimeProvider.UtcNow,
            storageSettings.MaxFileSizeBytes,
            null);

        if (mediaResult.IsFailure)
            return Result.Failure<Guid>(mediaResult.Error);

        mediaRepository.Add(mediaResult.Value);
        await storageService.SaveFileAsync(mediaContent.Content, storageKey, cancellationToken);

        var legalPaperResult = LegalPaper.Create(
            mediaResult.Value.Id,
            request.ApplicationId,
            request.OfficalName,
            AcceptanceStatus.UnderReview,
            request.Type,
            dateTimeProvider.UtcNow,
            request.ExpiryDateUtc);

        if (legalPaperResult.IsFailure)
            return Result.Failure<Guid>(legalPaperResult.Error);

        legalPapersRepository.Add(legalPaperResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await scheduler.ScheduleAsync(new LegalPaperExpiredDomainEvent(legalPaperResult.Value.Id), legalPaperResult.Value.ExpiryDateUtc, cancellationToken);

        return Result.Success(legalPaperResult.Value.Id);
    }
}