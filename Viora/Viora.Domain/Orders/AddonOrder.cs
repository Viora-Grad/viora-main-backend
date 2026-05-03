using Viora.Domain.Abstractions;
using Viora.Domain.Orders.Internal;
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

    private AddonOrder(Guid id, Guid organizationId, double totalPrice, DateTime createdDate, Guid subscriptionId, OrderStatus status)
        : base(id, organizationId, subscriptionId, totalPrice, createdDate, status)
    {
    }

    public static Result<AddonOrder> CreateAddonOrder(Guid organizationId, Guid subscriptionId, List<LimitedFeatureAddon> addons, DateTime createdDate)
    {
        var totalPrice = addons.Sum(a => a.Price);
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
