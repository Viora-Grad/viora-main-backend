using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Abstractions.Scheduling;
using Viora.Domain.Abstractions;
using Viora.Domain.WalletPromises;
using Viora.Domain.WalletPromises.Events;
using Viora.Domain.Wallets;
using Viora.Domain.Wallets.Internals;
using Viora.Domain.WalletTransactions.Internals;

namespace Viora.Application.Wallets.PromisePayment;

internal sealed class PromisePaymentCommandHandler(
    IWalletRepository walletRepository,
    IWalletPromiseRepository promiseRepository,
    IDomainEventScheduler eventScheduler,
    IWalletSettings walletSettings,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<PromisePaymentCommand, Guid>
{
    public async Task<Result<Guid>> Handle(PromisePaymentCommand request, CancellationToken cancellationToken)
    {
        var userWallet = await walletRepository.GetByOwnerAsync(request.UserId, WalletType.Customer, cancellationToken);
        if (userWallet is null)
            return Result.Failure<Guid>(WalletErrors.WalletNotFound);

        var branchWallet = await walletRepository.GetByOwnerAsync(request.BranchId, WalletType.Branch, cancellationToken);
        if (branchWallet is null)
            return Result.Failure<Guid>(WalletErrors.WalletBranchNotFound);

        var now = dateTimeProvider.UtcNow;
        var expiresAt = request.ReservationDate.AddMinutes(walletSettings.PromiseGraceMinutes);
        var promiseId = Guid.NewGuid();

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var lockedUser = await walletRepository.GetForUpdateAsync(userWallet.Id, cancellationToken);
        if (lockedUser is null)
            return Result.Failure<Guid>(WalletErrors.WalletNotFound);

        // Hold: debit the customer now; ref = promiseId so settlement/refund are idempotent against it.
        var hold = lockedUser.Debit(request.Amount, Purpose.Payment, "Appointment hold", promiseId.ToString(), now);
        if (hold.IsFailure)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<Guid>(hold.Error);
        }

        var promiseResult = WalletPromise.Create(promiseId, lockedUser.Id, branchWallet.Id, request.Amount, expiresAt, now, hold.Value.Id);
        if (promiseResult.IsFailure)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<Guid>(promiseResult.Error);
        }

        var promise = promiseResult.Value;

        // Schedule the expiry refund; store its id so check-in can cancel it.
        var scheduledEventId = await eventScheduler.ScheduleAsync(
            new PaymentPromisedEvent(promise.Id, expiresAt), expiresAt, cancellationToken);
        promise.AttachScheduledEvent(scheduledEventId);

        promiseRepository.Add(promise);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success(hold.Value.Id);
    }
}
