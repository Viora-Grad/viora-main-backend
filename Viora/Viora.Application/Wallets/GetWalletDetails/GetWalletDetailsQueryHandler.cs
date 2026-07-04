using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Wallets;
using Viora.Domain.Wallets.Internals;
using Viora.Domain.WalletTransactions;

namespace Viora.Application.Wallets.GetWalletDetails;

internal sealed class GetWalletDetailsQueryHandler(
    IWalletRepository walletRepository,
    IWalletTransactionsRepository transactionsRepository,
    IUserContext userContext) : IQueryHandler<GetWalletDetailsQuery, WalletDetailsResponse>
{
    public async Task<Result<WalletDetailsResponse>> Handle(GetWalletDetailsQuery request, CancellationToken cancellationToken)
    {
        var ownerId = request.Type == WalletType.Branch
            ? request.BranchId ?? Guid.Empty
            : userContext.UserId;

        var wallet = await walletRepository.GetByOwnerAsync(ownerId, request.Type, cancellationToken);
        if (wallet is null)
            return Result.Failure<WalletDetailsResponse>(WalletErrors.WalletNotFound);

        var transactions = await transactionsRepository.GetByWalletIdAsync(wallet.Id, request.Page, request.PageSize, cancellationToken);

        var response = new WalletDetailsResponse(
            wallet.Id,
            wallet.Type.ToString(),
            wallet.Balance.Amount,
            wallet.Currency.Code,
            transactions.Select(t => new WalletTransactionResponse(
                t.Id,
                t.Type.ToString(),
                t.Purpose.ToString(),
                t.Money.Amount,
                t.Money.Currency.Code,
                t.RunningBalance.Amount,
                t.Description.Value,
                t.ReferenceId.Value,
                t.CreatedAtUtc)).ToList());

        return Result.Success(response);
    }
}
