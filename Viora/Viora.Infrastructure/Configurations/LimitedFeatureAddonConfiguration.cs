using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Plans.Addons;
using Viora.Domain.Plans.Features;

namespace Viora.Infrastructure.Configurations;

internal sealed class LimitedFeatureAddonConfiguration : IEntityTypeConfiguration<LimitedFeatureAddon>
{
    public void Configure(EntityTypeBuilder<LimitedFeatureAddon> builder)
    {
        builder.ToTable("LimitedFeatureAddons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RestoreValue)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasPrecision(18, 2);

        builder.HasOne<LimitedFeature>()
            .WithMany()
            .HasForeignKey(x => x.LimitedFeatureId);
    }
}
