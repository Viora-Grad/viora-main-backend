using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Plans.Features;
using Viora.Domain.Plans.Features.Internal;

namespace Viora.Infrastructure.Configurations;

internal sealed class LimitedFeatureConfiguration : IEntityTypeConfiguration<LimitedFeature>
{
    public void Configure(EntityTypeBuilder<LimitedFeature> builder)
    {
        builder.ToTable("LimitedFeatures");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(
            v => v.value,
            v => new FeatureKey(v)
            );


        builder.Property(x => x.Description)
            .HasMaxLength(1000)
            .HasConversion(
            d => d.value,
            d => new FeatureDescription(d)
            );

        builder.Property(x => x.Limit)
            .IsRequired();
    }
}
