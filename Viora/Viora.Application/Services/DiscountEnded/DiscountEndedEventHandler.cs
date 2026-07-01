using MediatR;
using Microsoft.Extensions.Logging;
using Viora.Domain.Abstractions;
using Viora.Domain.Services;
using Viora.Domain.Services.Events;

namespace Viora.Application.Services.DiscountEnded;

/// <summary>
/// Fires when a service's discount period elapses (scheduled on the outbox by the add-discount flow).
/// Clears the now-expired discount from the service.
/// </summary>
internal sealed class DiscountEndedEventHandler(
    IServiceRepository serviceRepository,
    IUnitOfWork unitOfWork,
    ILogger<DiscountEndedEventHandler> logger) : INotificationHandler<DiscountEndedEvent>
{
    public async Task Handle(DiscountEndedEvent notification, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAsync(notification.ServiceId, cancellationToken);
        if (service is null)
        {
            logger.LogWarning(
                "DiscountEndedEvent: service {ServiceId} was not found; nothing to clear.",
                notification.ServiceId);
            return;
        }

        service.EndDiscount();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
