using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Security;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;

namespace Viora.Application.Marketing.SaveMetaCredential;

internal sealed class SaveMetaCredentialCommandHandler(
    IMetaPageCredentialRepository credentialRepository,
    IUserContext userContext,
    ICipher cipher,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<SaveMetaCredentialCommand>
{
    public async Task<Result> Handle(SaveMetaCredentialCommand request, CancellationToken cancellationToken)
    {
        if (userContext.OrganizationId is not { } organizationId)
            return Result.Failure(MarketingErrors.OrganizationMissing);

        var now = dateTimeProvider.UtcNow;

        // Encrypt at rest via the existing AES cipher; the plaintext token never touches the DB or logs.
        var encryptedToken = cipher.Encrypt(request.AccessToken);

        var existing = await credentialRepository.GetActiveByOrganizationAsync(organizationId, cancellationToken);
        if (existing is null)
        {
            var credential = MetaPageCredential.Create(organizationId, request.PageId, encryptedToken, now);
            credentialRepository.Add(credential);
        }
        else
        {
            existing.Update(request.PageId, encryptedToken, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
