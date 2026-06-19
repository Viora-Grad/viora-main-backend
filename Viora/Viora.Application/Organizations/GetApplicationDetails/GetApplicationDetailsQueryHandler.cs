using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Medias;
using Viora.Domain.Organizations.LegalPapers;
using Viora.Domain.Organizations.LegalPapers.Internals;
using Viora.Domain.Organizations.OnBoardings;
using Viora.Domain.Users.Identity;

namespace Viora.Application.Organizations.GetApplicationDetails;

internal sealed class GetApplicationDetailsQueryHandler(
    IOrganizationApplicationRepository applicationRepository,
    ILegalPaperRepository legalPaperRepository,
    IMediaRepository mediaRepository,
    IUserRepository userRepository) : IQueryHandler<GetApplicationDetailsQuery, ApplicationDetailsResponse>
{
    public async Task<Result<ApplicationDetailsResponse>> Handle(GetApplicationDetailsQuery request, CancellationToken cancellationToken)
    {
        var application = await applicationRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Application {request.Id} was not found.");

        var legalPapers = (await legalPaperRepository.GetByApplicationIdAsync(request.Id, cancellationToken)).ToList();

        var userIds = new HashSet<Guid> { application.OwnerId };

        if (application.RejectedBy.HasValue)
            userIds.Add(application.RejectedBy.Value);
        foreach (var lp in legalPapers.Where(lp => lp.ApprovedById.HasValue))
            userIds.Add(lp.ApprovedById!.Value);

        var mediaIds = legalPapers.Select(lp => lp.AttachmentId).ToList();

        var namesTask = userRepository.GetNamesDictAsync(userIds, cancellationToken);
        var mediasTask = mediaRepository.GetByIdsAsync(mediaIds, cancellationToken);

        await Task.WhenAll(namesTask, mediasTask);

        var names = await namesTask;
        var mediaDict = (await mediasTask).ToDictionary(m => m.Id);

        var response = new ApplicationDetailsResponse(
            application.Id,
            application.OwnerId,
            names.GetValueOrDefault(application.OwnerId, string.Empty),
            application.ProposedName.Value,
            application.About.Value,
            application.ApplicationLetter.Value,
            application.ServiceDescription.Value,
            application.ProposedServicesType.Select(s => s.Value),
            application.SubmittedOnUtc,
            application.Status.ToString(),
            application.ReferralSource.ToString(),
            application.RejectedBy,
            application.RejectedBy.HasValue ? names.GetValueOrDefault(application.RejectedBy.Value) : null,
            application.ExpiryDateUtc,
            application.BillingEmail.Value,
            application.SupportEmail.Value,
            MapLegalPaper(legalPapers, LegalPaperType.ArticleOfAssociation, mediaDict, names),
            MapLegalPaper(legalPapers, LegalPaperType.CommercialRegistration, mediaDict, names),
            MapLegalPaper(legalPapers, LegalPaperType.RegisteredAddressProof, mediaDict, names),
            MapLegalPaper(legalPapers, LegalPaperType.TaxCard, mediaDict, names));

        return Result.Success(response);
    }

    private static LegalPaper? MapLegalPaper(
        List<Domain.Organizations.LegalPapers.LegalPaper> papers,
        LegalPaperType type,
        Dictionary<Guid, MediaFile> mediaDict,
        IReadOnlyDictionary<Guid, string> names)
    {
        var paper = papers.FirstOrDefault(p => p.Type == type);
        if (paper is null) return null;

        if (!mediaDict.TryGetValue(paper.AttachmentId, out var media))
            return null;

        var mediaResponse = new MediaResponse(media.Id, media.MimeType.Value, media.Name.Value, media.UploadedAtUtc);

        Approval? approval = paper.ApprovedById.HasValue
            ? new Approval(paper.ApprovedById.Value, names.GetValueOrDefault(paper.ApprovedById.Value, string.Empty))
            : null;

        return new LegalPaper(
            paper.Id,
            paper.Name.Value,
            mediaResponse,
            approval,
            paper.SubmissionDateUtc,
            paper.ExpiryDateUtc);
    }
}
