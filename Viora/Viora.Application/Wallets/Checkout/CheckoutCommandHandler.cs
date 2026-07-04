using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Billings;
using Viora.Domain.Abstractions;
using Viora.Domain.Shared;
using Viora.Domain.Wallets;
using Viora.Domain.Wallets.Internals;
using Viora.Domain.WalletTransactions.Internals;

namespace Viora.Application.Wallets.Checkout;

internal sealed class CheckoutCommandHandler(
    IWalletRepository walletRepository,
    IPaymentService paymentService,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<CheckoutCommand, CheckoutResponse>
{
    public async Task<Result<CheckoutResponse>> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        var branchWallet = await walletRepository.GetByOwnerAsync(request.BranchId, WalletType.Branch, cancellationToken);
        if (branchWallet is null)
            return Result.Failure<CheckoutResponse>(WalletErrors.WalletNotFound);

        var amount = new Money(request.Amount, Currency.FromCode(request.Currency));
        var now = dateTimeProvider.UtcNow;

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var locked = await walletRepository.GetForUpdateAsync(branchWallet.Id, cancellationToken);
        if (locked is null)
            return Result.Failure<CheckoutResponse>(WalletErrors.WalletNotFound);

        var canCheckout = locked.EnsureCanCheckout();
        if (canCheckout.IsFailure)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<CheckoutResponse>(canCheckout.Error);
        }

        // Pre-check balance so we don't call the gateway for an amount we can't cover.
        if (locked.Balance.Amount < amount.Amount)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<CheckoutResponse>(WalletErrors.InsufficientFunds);
        }

        var merchantTransferId = Guid.NewGuid().ToString();
        var payoutRequest = new PayoutRequest
        {
            Amount = amount.Amount,
            RecipientName = request.RecipientName,
            RecipientBank = request.RecipientBank,
            RecipientNumber = request.RecipientNumber,
            MerchantTransferId = merchantTransferId,
        };

        // The gateway transfer is bypassed (fire-and-ignore), so this always succeeds; the debit proceeds.
        var payout = await paymentService.InitiatePayoutAsync(payoutRequest, cancellationToken);
        if (payout.IsFailure)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<CheckoutResponse>(payout.Error);
        }

        var debit = locked.Debit(amount, Purpose.Payout, "Branch checkout", payout.Value.Reference, now);
        if (debit.IsFailure)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<CheckoutResponse>(debit.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success(new CheckoutResponse(payout.Value.Reference));
    }
}
