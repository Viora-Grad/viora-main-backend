using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Billings.Invoices;
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

        builder.HasMany<AddonOrderLimitedFeature>()
            .WithOne()
            .HasForeignKey(x => x.AddonOrderId);

        builder.HasOne<Invoice>()
            .WithMany()
            .HasForeignKey(o => o.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
