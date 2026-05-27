using MediatR;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Application.Abstractions.Scheduling;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.Suspensions;
using Viora.Domain.Organizations.Suspensions.Events;

namespace Viora.Application.Organizations.OrganizationDeletionDue;

internal class OrganizationDeletionEventHandler(
    ISuspensionRepository suspensionRepository,
    IOrganizationRepository organizationRepository,
    IDateTimeProvider dateTimeProvider,
    IDomainEventScheduler scheduler) : INotificationHandler<OrganizationDeletionDueEvent>
{
    public async Task Handle(OrganizationDeletionDueEvent notification, CancellationToken cancellationToken)
    {
        var suspension = await suspensionRepository.GetByIdAsync(notification.OrganizationId, cancellationToken) ??
            throw new NotFoundException($"Organization {notification.OrganizationId} was not found");

        if (suspension.ScheduledDeletionDateUtc > dateTimeProvider.UtcNow)
        {
            await scheduler.ScheduleAsync(new OrganizationDeletionDueEvent(notification.OrganizationId), suspension.ScheduledDeletionDateUtc, cancellationToken);
            return;
        }

        var org = await organizationRepository.GetByIdAsync(notification.OrganizationId, cancellationToken);

        // TODO determine the deletion behavior hard to save the data or not
    }
}