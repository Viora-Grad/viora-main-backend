using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Internal;

namespace Viora.Infrastructure.Configurations;

internal sealed class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasConversion(
                name => name.value,
                value => new PlanName(value));

        builder.Property(p => p.Description)
            .HasMaxLength(1000)
            .HasConversion(
                description => description.Value,
                value => new PlanDescription(value));

        builder.ComplexProperty(s => s.Price, mb =>
        {
            mb.Property(m => m.Amount)
                .HasColumnName("PriceAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            mb.ComplexProperty(m => m.Currency, cb =>
            {
                cb.Property(c => c.Code)
                    .HasColumnName("PriceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });
        });

        builder.Property(x => x.PlanPeriod)
            .HasConversion(
                v => v.Id,
                v => PlanPeriod.FromId(v)
            )
            .HasColumnName("PlanPeriodId");

        builder.Property(p => p.Content)
            .HasConversion(
                content => content.Value,
                value => new PlanContent(value));


        builder.HasMany(p => p.PlanFeatures)
            .WithOne()
            .HasForeignKey(pf => pf.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.PlanLimitedFeatures)
            .WithOne()
            .HasForeignKey(plf => plf.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
