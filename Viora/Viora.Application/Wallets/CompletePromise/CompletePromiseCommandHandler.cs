using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Scheduling;
using Viora.Domain.Abstractions;
using Viora.Domain.WalletPromises;
using Viora.Domain.WalletPromises.Internals;
using Viora.Domain.Wallets;
using Viora.Domain.WalletTransactions.Internals;

namespace Viora.Application.Wallets.CompletePromise;

internal sealed class CompletePromiseCommandHandler(
    IWalletPromiseRepository promiseRepository,
    IWalletRepository walletRepository,
    IDomainEventScheduler eventScheduler,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<CompletePromiseCommand>
{
    public async Task<Result> Handle(CompletePromiseCommand request, CancellationToken cancellationToken)
    {
        var promise = await promiseRepository.GetByIdAsync(request.PromiseId, cancellationToken);
        if (promise is null)
            return Result.Failure(WalletPromiseErrors.NotFound);

        // Idempotent: already settled/refunded -> no-op.
        if (promise.Status != PromiseStatus.Pending)
            return Result.Success();

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var branchWallet = await walletRepository.GetForUpdateAsync(promise.ToWalletId, cancellationToken);
        if (branchWallet is null)
            return Result.Failure(WalletErrors.WalletNotFound);

        var credit = branchWallet.Credit(promise.Money, Purpose.Payment, "Appointment settlement", promise.Id.ToString(), dateTimeProvider.UtcNow);
        if (credit.IsFailure)
        {
            await transaction.RollbackAsync(cancellationToken);
            return credit;
        }

        var completed = promise.Complete(credit.Value.Id);
        if (completed.IsFailure)
        {
            await transaction.RollbackAsync(cancellationToken);
            return completed;
        }

        // Check-in won the race: cancel the pending expiry refund.
        if (promise.ScheduledEventId.HasValue)
            await eventScheduler.CancelAsync(promise.ScheduledEventId.Value, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
