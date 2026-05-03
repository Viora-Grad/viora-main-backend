using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Plans.Features;
using Viora.Domain.Plans.Features.Internal;

namespace Viora.Infrastructure.Configurations;

internal sealed class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder.ToTable("Features");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FeatureKey)
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(
                v => v.value,
                v => new FeatureKey(v));

        builder.Property(x => x.Description)
            .HasMaxLength(1000)
            .HasConversion(
                v => v.value,
                v => new FeatureDescription(v));
    }
}
