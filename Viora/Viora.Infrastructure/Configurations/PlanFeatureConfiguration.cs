using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Plans;
using Viora.Domain.Plans.Features;

namespace Viora.Infrastructure.Configurations;

internal sealed class PlanFeatureConfiguration : IEntityTypeConfiguration<PlanFeature>
{
    public void Configure(EntityTypeBuilder<PlanFeature> builder)
    {
        builder.ToTable("PlanFeatures");

        builder.HasKey(x => x.Id);

        builder.HasOne<Plan>()
            .WithMany()
            .HasForeignKey(x => x.PlanId);

        builder.HasOne<Feature>()
            .WithMany()
            .HasForeignKey(x => x.FeatureId);

        builder.HasOne<LimitedFeature>()
            .WithMany()
            .HasForeignKey(x => x.LimitedFeatureId)
            .IsRequired(false);
    }
}
