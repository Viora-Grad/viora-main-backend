using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Orders;
using Viora.Domain.Orders.Internal;

namespace Viora.Infrastructure.Configurations;

internal sealed class SubscriptionOrderConfiguration : IEntityTypeConfiguration<SubscriptionOrder>
{
    public void Configure(EntityTypeBuilder<SubscriptionOrder> builder)
    {
        builder.Property(s => s.Status)
            .HasConversion(
            v => v.id,
            v => OrderStatus.FromId(v)
            );

        builder.Property(s => s.TotalPrice)
            .HasPrecision(18, 2);

        builder.Property(s => s.PlanId)
            .IsRequired();

        builder.Property(s => s.SubscriptionOrderType)
            .IsRequired()
            .HasConversion(
            x => x.Value,
            x => SubscriptionOrderType.FromValue(x).Value);
    }
}
