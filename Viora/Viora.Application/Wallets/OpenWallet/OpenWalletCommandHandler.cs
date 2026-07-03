using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Abstractions;
using Viora.Domain.Shared;
using Viora.Domain.Wallets;
using Viora.Domain.Wallets.Internals;

namespace Viora.Application.Wallets.OpenWallet;

internal sealed class OpenWalletCommandHandler(
    IWalletRepository walletRepository,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork) : ICommandHandler<OpenWalletCommand, Guid>
{
    public async Task<Result<Guid>> Handle(OpenWalletCommand request, CancellationToken cancellationToken)
    {
        // Ownership: a customer wallet belongs to the caller; a branch wallet requires a branch id.
        var ownerId = request.Type == WalletType.Branch
            ? request.BranchId ?? Guid.Empty
            : userContext.UserId;

        if (ownerId == Guid.Empty)
            return Result.Failure<Guid>(WalletErrors.WalletNotFound);

        if (await walletRepository.ExistsForOwnerAsync(ownerId, request.Type, cancellationToken))
            return Result.Failure<Guid>(WalletErrors.WalletAlreadyExists);

        var wallet = Wallet.Create(ownerId, request.Type, Currency.Egp, dateTimeProvider.UtcNow);
        walletRepository.Add(wallet);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(wallet.Id);
    }
}
