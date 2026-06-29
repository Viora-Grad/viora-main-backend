using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Billings.Invoices;
using Viora.Domain.Billings.Invoices.Internals;
using Viora.Domain.Shared;
using Viora.Domain.Shared.Internal;

namespace Viora.Infrastructure.Configurations;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedNever();

        builder.Property(i => i.OrganizationId)
            .IsRequired();

        builder.Property(i => i.OrganizationName)
            .HasConversion(name => name.Value, value => new OrganizationName(value))
            .HasColumnName("OrganizationName")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.BillTo)
            .HasConversion(email => email.Value, value => new Email(value))
            .HasColumnName("BillTo")
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(i => i.Currency)
            .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
            .HasColumnName("Currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(i => i.CreatedAtUtc)
            .IsRequired();

        builder.Property(i => i.DueDateUtc)
            .IsRequired(false);

        builder.Property(i => i.TaxPercentage)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property<long>("Sequence")
            .HasColumnName("Sequence")
            .IsRequired();

        builder.Ignore(i => i.Number);

        builder.Ignore(i => i.SubTotal);
        builder.Ignore(i => i.TotalDiscount);
        builder.Ignore(i => i.NetTotal);
        builder.Ignore(i => i.TotalTax);
        builder.Ignore(i => i.Total);

        builder.Ignore(i => i.Items);
        builder.OwnsMany<InvoiceItem>("_items", item =>
        {
            item.ToTable("InvoiceItems");
            item.WithOwner().HasForeignKey("InvoiceId");

            item.Property<int>("_number")
                .HasColumnName("ItemNumber");
            item.Ignore(i => i.ItemNumber);

            item.Property(i => i.ItemName)
                .HasColumnName("ItemName")
                .HasMaxLength(200)
                .IsRequired();

            item.Property(i => i.Description)
                .HasColumnName("Description")
                .HasMaxLength(1000)
                .IsRequired();

            item.Property(i => i.Quantity)
                .IsRequired();

            item.Property(i => i.DiscountPercentage)
                .HasPrecision(18, 6)
                .IsRequired();

            item.Property(i => i.TaxPercentage)
                .HasPrecision(18, 6)
                .IsRequired();

            item.OwnsOne(i => i.Price, price =>
            {
                price.Property(money => money.Amount)
                    .HasColumnName("PriceAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();

                price.Property(money => money.Currency)
                    .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                    .HasColumnName("PriceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            // Per-item money breakdown is derived and not persisted.
            item.Ignore(i => i.SubTotal);
            item.Ignore(i => i.DiscountAmount);
            item.Ignore(i => i.NetAmount);
            item.Ignore(i => i.TaxAmount);
            item.Ignore(i => i.TotalAmount);

            item.UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.ComplexProperty(x => x.ExternalPayment, complex =>
        {
            complex.Property(p => p.Id)
                .HasColumnName("ExternalPaymentId")
                .HasMaxLength(50);

            complex.Property(p => p.Url)
                .HasColumnName("ExternalPaymentUrl")
                .HasMaxLength(300);
        });
    }
}
