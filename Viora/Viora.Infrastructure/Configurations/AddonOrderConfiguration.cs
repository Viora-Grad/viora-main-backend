using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Orders;
using Viora.Domain.Orders.Internal;
using Viora.Domain.Subscriptions.Addons;

namespace Viora.Infrastructure.Configurations;

internal sealed class AddonOrderConfiguration : IEntityTypeConfiguration<AddonOrder>
{
    public void Configure(EntityTypeBuilder<AddonOrder> builder)
    {

        builder.Property(s => s.Status)
            .HasConversion(
            v => v.id,
            v => OrderStatus.FromId(v)
            );

        builder.ToTable("AddonOrders");

        builder.HasMany<AddonOrderLimitedFeature>()
            .WithOne()
            .HasForeignKey(x => x.AddonOrderId);
    }
}
