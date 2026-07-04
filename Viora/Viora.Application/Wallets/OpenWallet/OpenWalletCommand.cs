using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Wallets.Internals;

namespace Viora.Application.Wallets.OpenWallet;

/// <summary>
/// Opens a wallet for the caller. For a customer wallet the owner is the authenticated user; for a
/// branch wallet the owner is <paramref name="BranchId"/> (owner/staff authorized).
/// </summary>
public sealed record OpenWalletCommand(WalletType Type, Guid? BranchId) : ICommand<Guid>;
