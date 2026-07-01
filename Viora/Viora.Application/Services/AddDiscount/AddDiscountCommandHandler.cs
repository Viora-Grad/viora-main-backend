using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Scheduling;
using Viora.Domain.Abstractions;
using Viora.Domain.Services;
using Viora.Domain.Services.Events;

namespace Viora.Application.Services.AddDiscount;

internal sealed class AddDiscountCommandHandler(
    IServiceRepository serviceRepository,
    IDomainEventScheduler eventScheduler,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<AddDiscountCommand>
{
    public async Task<Result> Handle(AddDiscountCommand request, CancellationToken cancellationToken)
    {
        var service = await serviceRepository.GetByIdAsync(request.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure(ServiceErrors.NotFound);

        var now = dateTimeProvider.UtcNow;

        var discountResult = service.AddDiscount(request.DiscountOutOf100, request.Reason, request.Duration, now);
        if (discountResult.IsFailure)
            return discountResult;

        // Schedule the discount to be cleared when it expires. Enqueued on the outbox in the same
        // transaction as the discount itself, so the two commit atomically.
        await eventScheduler.ScheduleAsync(new DiscountEndedEvent(service.Id), now + request.Duration, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
