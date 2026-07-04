using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;
using Viora.Application.Abstractions.Authentication;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Messaging;
using Viora.Application.Billings;
using Viora.Domain.Abstractions;
using Viora.Domain.Billings;
using Viora.Domain.Wallets;
using Viora.Domain.Wallets.Internals;

namespace Viora.Application.Wallets.RechargeOrder;

internal sealed class CreateRechargeSessionCommandHandler(
    IWalletRepository walletRepository,
    IPaymentService paymentService,
    IPaymentSettings paymentSettings,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider,
    ILogger<CreateRechargeSessionCommandHandler> logger) : ICommandHandler<CreateRechargeSessionCommand, CreateRechargeSessionResponse>
{
    private static readonly JsonSerializerOptions LogJsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    public async Task<Result<CreateRechargeSessionResponse>> Handle(CreateRechargeSessionCommand request, CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;

        var wallet = await walletRepository.GetByOwnerAsync(userId, WalletType.Customer, cancellationToken);
        if (wallet is null)
            return Result.Failure<CreateRechargeSessionResponse>(WalletErrors.WalletNotFound);

        //var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        //if (user is null)
        //    return Result.Failure<CreateRechargeSessionResponse>(WalletErrors.WalletNotFound);

        var now = dateTimeProvider.UtcNow;
        logger.LogInformation("Wallet recharge session requested. User={UserId} Amount={Amount}", userId, request.Amount);

        // Kashier requires a unique order id per session, so a bare user id (reused across recharges) is
        // rejected. We build the order as [user id ("N", 32 hex chars)] + [fresh guid ("N")]: unique per
        // attempt, purely alphanumeric, and the webhook recovers the wallet from the fixed 32-char prefix.
        var order = userId.ToString("N") + Guid.NewGuid().ToString("N");

        var paymentRequest = new PaymentRequest
        {
            Amount = request.Amount.ToString("0.00", CultureInfo.InvariantCulture),
            Currency = wallet.Currency.Code,
            Order = order,
            MerchantId = paymentSettings.MerchentId,
            ServerWebhook = $"{paymentSettings.PublicBaseUrl.TrimEnd('/')}/api/webhooks/kashier/recharge",
            MerchantRedirect = paymentSettings.ClientBaseUrl,
            Customer = new PaymentCustomer("test@gmail.com", userId.ToString()),
            ExpireAt = now.AddMinutes(30),
            Description = $"Wallet recharge for {userId}",
            Notes = "Wallet recharge order",
            BrandColor = "#201335"
        };

        logger.LogInformation("Recharge Kashier request body:\n{Body}", JsonSerializer.Serialize(paymentRequest, LogJsonOptions));

        var session = await paymentService.CreatePaymentSessionAsync(paymentRequest, cancellationToken);
        if (session.IsFailure)
            return Result.Failure<CreateRechargeSessionResponse>(session.Error);

        return Result.Success(new CreateRechargeSessionResponse(session.Value.SessionUrl));
    }
}
