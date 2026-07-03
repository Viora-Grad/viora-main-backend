using Viora.Application.Abstractions.Messaging;
using Viora.Domain.Wallets.Internals;

namespace Viora.Application.Wallets.GetWalletDetails;

public sealed record GetWalletDetailsQuery(WalletType Type, Guid? BranchId, int Page = 1, int PageSize = 20)
    : IQuery<WalletDetailsResponse>;
