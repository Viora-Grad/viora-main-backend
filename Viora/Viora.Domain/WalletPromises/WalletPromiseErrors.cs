using Viora.Domain.Abstractions;

namespace Viora.Domain.WalletPromises;

public static class WalletPromiseErrors
{
    public static readonly Error CannotTransferToSelf = new("WalletPromises.CannotTransferToSelf", "Can not transfer to your self", ErrorCategory.Validation);
    public static readonly Error AmountLessThanZero = new("WalletPromises.AmountLessThanZero", " Amount can not be less than zero", ErrorCategory.Validation);
    public static readonly Error InvalidExpirationTime = new("WalletPromises.InvalidExpirationTime", "Expiry can not be less than current date", ErrorCategory.Validation);
    public static readonly Error NotFound = new("WalletPromises.NotFound", "The wallet promise was not found", ErrorCategory.NotFound);
    public static readonly Error AlreadyResolved = new("WalletPromises.AlreadyResolved", "The promise has already been resolved", ErrorCategory.Conflict);
    public static readonly Error InvalidStatusTransition = new("WalletPromises.InvalidStatusTransition", "Invalid promise status transition", ErrorCategory.Conflict);
}
