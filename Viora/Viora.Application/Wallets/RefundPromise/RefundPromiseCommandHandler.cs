using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Scheduling;
using Viora.Domain.Abstractions;
using Viora.Domain.WalletPromises;
using Viora.Domain.WalletPromises.Internals;
using Viora.Domain.Wallets;
using Viora.Domain.WalletTransactions.Internals;

namespace Viora.Application.Wallets.RefundPromise;

internal sealed class RefundPromiseCommandHandler(
    IWalletPromiseRepository promiseRepository,
    IWalletRepository walletRepository,
    IDomainEventScheduler eventScheduler,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<RefundPromiseCommand>
{
    public async Task<Result> Handle(RefundPromiseCommand request, CancellationToken cancellationToken)
    {
        var promise = await promiseRepository.GetByIdAsync(request.PromiseId, cancellationToken);
        if (promise is null)
            return Result.Failure(WalletPromiseErrors.NotFound);

        // Idempotent: already settled/refunded -> no-op.
        if (promise.Status != PromiseStatus.Pending)
            return Result.Success();

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var userWallet = await walletRepository.GetForUpdateAsync(promise.FromWalletId, cancellationToken);
        if (userWallet is null)
            return Result.Failure(WalletErrors.WalletNotFound);

        var credit = userWallet.Credit(promise.Money, Purpose.Refund, "Appointment refund", promise.Id.ToString(), dateTimeProvider.UtcNow);
        if (credit.IsFailure)
        {
            await transaction.RollbackAsync(cancellationToken);
            return credit;
        }

        var refunded = promise.Refund();
        if (refunded.IsFailure)
        {
            await transaction.RollbackAsync(cancellationToken);
            return refunded;
        }

        // If refund was triggered by cancel/no-show (before expiry), cancel the pending expiry event.
        if (promise.ScheduledEventId.HasValue)
            await eventScheduler.CancelAsync(promise.ScheduledEventId.Value, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
