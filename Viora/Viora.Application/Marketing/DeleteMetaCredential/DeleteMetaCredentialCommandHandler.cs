using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Marketing;

namespace Viora.Application.Marketing.DeleteMetaCredential;

internal sealed class DeleteMetaCredentialCommandHandler(
    IMetaPageCredentialRepository credentialRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteMetaCredentialCommand>
{
    public async Task<Result> Handle(DeleteMetaCredentialCommand request, CancellationToken cancellationToken)
    {
        if (userContext.OrganizationId is not { } organizationId)
            return Result.Failure(MarketingErrors.OrganizationMissing);

        var credential = await credentialRepository.GetActiveByOrganizationAsync(organizationId, cancellationToken);
        if (credential is null)
            return Result.Success(); // nothing to delete — idempotent

        credentialRepository.Remove(credential);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
