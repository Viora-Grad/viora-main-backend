using Viora.Domain.Abstractions;
using Viora.Domain.Orders.Internal;
using Viora.Domain.Shared;
using Viora.Domain.Subscriptions.Addons;

namespace Viora.Domain.Orders;

public class AddonOrder : Order
{
    private List<LimitedFeatureAddon> _addons { get; } = new List<LimitedFeatureAddon>();

    public IReadOnlyCollection<LimitedFeatureAddon> Addons => _addons.AsReadOnly();

    private AddonOrder()
    {
        // Required by EF Core
    }

    private AddonOrder(Guid id, Guid organizationId, Money totalPrice, DateTime createdDate, Guid subscriptionId, OrderStatus status)
        : base(id, organizationId, subscriptionId, totalPrice, createdDate, status)
    {
    }

    public static Result<AddonOrder> CreateAddonOrder(Guid organizationId, Guid subscriptionId, List<LimitedFeatureAddon> addons, DateTime createdDate)
    {
        if (addons is null || addons.Count == 0)
        {
            return Result.Failure<AddonOrder>(
                OrderError.NoAddon);
        }
        Money totalPrice = addons[0].Price;

        foreach (var addon in addons.Skip(1))
        {
            totalPrice += addon.Price;
        }
        var newAddonOrder = new AddonOrder(
            Guid.NewGuid(),
            organizationId,
            totalPrice,
            createdDate,
            subscriptionId,
            OrderStatus.Pending
        );
        newAddonOrder._addons.AddRange(addons);
        // Raise the orderPaidEvent 
        return Result.Success(newAddonOrder);
    }
}
