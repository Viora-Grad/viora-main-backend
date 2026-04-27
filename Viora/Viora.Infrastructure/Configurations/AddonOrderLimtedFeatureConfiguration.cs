using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Subscriptions.Addons;

namespace Viora.Infrastructure.Configurations;

internal sealed class AddonOrderLimitedFeatureConfiguration : IEntityTypeConfiguration<AddonOrderLimitedFeature>
{
    public void Configure(EntityTypeBuilder<AddonOrderLimitedFeature> builder)
    {
        builder.ToTable("AddonOrderLimitedFeatures");

        builder.HasKey(x => new { x.AddonOrderId, x.LimitedFeatureId });
    }
}