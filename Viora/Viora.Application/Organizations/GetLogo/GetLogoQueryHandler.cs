using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Media;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Medias;
using Viora.Domain.Organizations.OrganizationDetails;

namespace Viora.Application.Organizations.GetLogo;

internal class GetLogoQueryHandler(
    IOrganizationRepository organizationRepository,
    IMediaRepository mediaRepository,
    IStorageService storageService) : IQueryHandler<GetLogoQuery, MediaResponseStream>
{
    public async Task<Result<MediaResponseStream>> Handle(GetLogoQuery request, CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken)
            ?? throw new NotFoundException($"Organization {request.OrganizationId} not found");

        var logoId = organization.LogoId ?? throw new NotFoundException("Organization does not have logo");

        var media = await mediaRepository.GetByIdAsync(logoId, cancellationToken) ?? throw new NotFoundException($"Media {logoId} not found");

        var stream = storageService.GetFileStream(media.Key);
        return Result.Success(new MediaResponseStream(stream, media.MimeType.Value, media.Name.Value));
    }
}
