using Microsoft.Extensions.Logging;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Shared;
using Viora.Domain.Wallets;
using Viora.Domain.Wallets.Internals;
using Viora.Domain.WalletTransactions;
using Viora.Domain.WalletTransactions.Internals;

namespace Viora.Application.Wallets.ProvisionRecharge;

internal sealed class ProvisionRechargeCommandHandler(
    IWalletRepository walletRepository,
    IWalletTransactionsRepository transactionsRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    ILogger<ProvisionRechargeCommandHandler> logger) : ICommandHandler<ProvisionRechargeCommand>
{
    public async Task<Result> Handle(ProvisionRechargeCommand request, CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByOwnerAsync(request.UserId, WalletType.Customer, cancellationToken);
        if (wallet is null)
        {
            // No wallet to credit — recharge requires an opened wallet. Log and swallow (webhook returns 200).
            logger.LogError("Recharge provisioning: no wallet for user {UserId}; dropping recharge ref {Reference}.",
                request.UserId, request.ReferenceId);
            return Result.Success();
        }

        // Idempotency: a replayed webhook for the same gateway transaction is a no-op.
        if (await transactionsRepository.ExistsAsync(TransactionType.Credit, Purpose.Recharge, request.ReferenceId, cancellationToken))
        {
            logger.LogInformation("Recharge provisioning: ref {Reference} already applied; ignoring duplicate.", request.ReferenceId);
            return Result.Success();
        }

        var amount = new Money(request.Amount, Currency.FromCode(request.Currency));

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var locked = await walletRepository.GetForUpdateAsync(wallet.Id, cancellationToken);
        if (locked is null)
            return Result.Success();

        var credit = locked.Credit(amount, Purpose.Recharge, "Wallet recharge", request.ReferenceId, dateTimeProvider.UtcNow);
        if (credit.IsFailure)
        {
            await transaction.RollbackAsync(cancellationToken);
            return credit;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
