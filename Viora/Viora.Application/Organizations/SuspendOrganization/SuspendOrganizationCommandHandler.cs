using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Scheduling;
using Viora.Domain.Abstractions;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.Suspensions;
using Viora.Domain.Organizations.Suspensions.Events;
using Viora.Domain.Organizations.Suspensions.Internals;

namespace Viora.Application.Organizations.SuspendOrganization;

internal class SuspendOrganizationCommandHandler(
    IOrganizationRepository organizationRepository,
    ISuspensionRepository suspensionRepository,
    ISuspensionSettings suspensionSettings,
    IUnitOfWork unitOfWork,
    IDomainEventScheduler scheduler,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<SuspendOrganizationCommand>
{
    public async Task<Result> Handle(SuspendOrganizationCommand request, CancellationToken cancellationToken)
    {
        var organizationToBeSuspended = await organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken) ??
            throw new NotFoundException($"Organization with Id {request.OrganizationId} was not found");

        var suspensionResult = organizationToBeSuspended.Suspend();
        if (suspensionResult.IsFailure)
            return suspensionResult;

        var suspensionRecordResult = Suspension.Create(
            organizationToBeSuspended.Id,
            organizationToBeSuspended.OwnerId,
            organizationToBeSuspended.Name,
            request.SuspendedById,
            Enum.Parse<SuspensionReason>(request.Reason),
            request.Notes,
            dateTimeProvider.UtcNow,
            suspensionSettings);

        if (suspensionRecordResult.IsFailure)
            return Result.Failure(suspensionRecordResult.Error);

        var suspension = suspensionRecordResult.Value;

        suspensionRepository.Add(suspension);

        await scheduler.ScheduleAsync(
            new OrganizationDeletionDueEvent(suspension.Id),
            suspension.ScheduledDeletionDateUtc,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
