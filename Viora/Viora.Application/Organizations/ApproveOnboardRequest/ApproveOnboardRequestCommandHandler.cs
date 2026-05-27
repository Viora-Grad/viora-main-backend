using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OnBoardings;
using Viora.Domain.Organizations.OrganizationDetails;

namespace Viora.Application.Organizations.ApproveOnboardRequest;

internal class ApproveOnboardRequestCommandHandler(
    IOrganizationApplicationRepository applicationRepository,
    IOrganizationRepository organizationRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<ApproveOnboardRequestCommand, Guid>
{
    public async Task<Result<Guid>> Handle(ApproveOnboardRequestCommand request, CancellationToken cancellationToken)
    {
        var application = await applicationRepository.GetByIdAsync(request.RequestId, cancellationToken)
            ?? throw new NotFoundException($"Organization application with ID {request.RequestId} not found.");

        var markResult = application.MarkAccepted(dateTimeProvider.UtcNow);

        if (markResult.IsFailure)
            return Result.Failure<Guid>(markResult.Error);

        var result = Organization.Create(
            application.OwnerId,
            application.CountryId,
            application.ProposedName,
            application.About,
            application.ServiceDescription,
            application.ProposedServicesType,
            dateTimeProvider.UtcNow,
            application.ReferralSource,
            application.BillingEmail,
            application.SupportEmail);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        organizationRepository.Add(result.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(result.Value.Id);
    }
}