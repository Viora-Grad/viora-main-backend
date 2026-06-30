using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Billings.Invoices;
using Viora.Domain.Orders;
using Viora.Domain.Orders.Internal;
using Viora.Domain.Plans;

namespace Viora.Infrastructure.Configurations;

internal sealed class SubscriptionOrderConfiguration : IEntityTypeConfiguration<SubscriptionOrder>
{
    public void Configure(EntityTypeBuilder<SubscriptionOrder> builder)
    {
        builder.ToTable("SubscriptionOrder");

        builder.Property(s => s.Status)
            .HasConversion(
            v => v.Id,
            v => OrderStatus.FromId(v)
            );

        builder.ComplexProperty(s => s.TotalPrice, mb =>
        {
            mb.Property(m => m.Amount)
                .HasColumnName("TotalPriceAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            mb.ComplexProperty(m => m.Currency, cb =>
            {
                cb.Property(c => c.Code)
                    .HasColumnName("TotalPriceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });
        });

        builder.Property(s => s.PlanId)
            .IsRequired();

        builder.Property(s => s.SubscriptionOrderType)
            .IsRequired()
            .HasConversion(
            x => x.Value,
            x => SubscriptionOrderType.FromValue(x).Value);

        builder.HasOne<Plan>()
            .WithMany()
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Invoice>()
            .WithMany()
            .HasForeignKey(o => o.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
