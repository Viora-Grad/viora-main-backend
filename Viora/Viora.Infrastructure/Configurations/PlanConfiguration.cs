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

        builder.Property(p => p.Price)
            .HasPrecision(18, 2);

        builder.Property(p => p.PlanPeriod)
            .IsRequired();

        builder.Property(p => p.Content)
            .HasConversion(
                content => content.Value,
                value => new PlanContent(value));
    }
}
