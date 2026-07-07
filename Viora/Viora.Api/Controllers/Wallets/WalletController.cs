using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Viora.Api.Extensions;
using Viora.Application.Wallets.Checkout;
using Viora.Application.Wallets.GetWalletDetails;
using Viora.Application.Wallets.OpenWallet;
using Viora.Application.Wallets.RechargeOrder;
using Viora.Domain.Wallets.Internals;

namespace Viora.Api.Controllers.Wallets;

[Route("api/wallets")]
[ApiController]
[Authorize]
public class WalletController(ISender sender) : ControllerBase
{
    // --- Customer wallet ---

    [HttpPost("customer")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> OpenCustomerWallet(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new OpenWalletCommand(WalletType.Customer, null), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("customer")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> GetCustomerWallet(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetWalletDetailsQuery(WalletType.Customer, null, page, pageSize), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("customer/recharge")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Recharge(RechargeRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateRechargeSessionCommand(request.Amount), cancellationToken);
        return result.ToActionResult();
    }

    // --- Branch wallet ---

    [HttpPost("branch/{branchId:guid}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> OpenBranchWallet(Guid branchId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new OpenWalletCommand(WalletType.Branch, branchId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("branch/{branchId:guid}")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> GetBranchWallet(Guid branchId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetWalletDetailsQuery(WalletType.Branch, branchId, page, pageSize), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("branch/{branchId:guid}/checkout")]
    [Authorize(Roles = "Owner")]
    public async Task<IActionResult> Checkout(Guid branchId, CheckoutRequest request, CancellationToken cancellationToken)
    {
        var command = new CheckoutCommand(branchId, request.Amount, request.Currency, request.RecipientName, request.RecipientBank, request.RecipientNumber);
        var result = await sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}
