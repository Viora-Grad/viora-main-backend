using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Plans;

namespace Viora.Infrastructure.Configurations;

internal sealed class PlanLimitedFeatureConfiguration : IEntityTypeConfiguration<PlanLimitedFeature>
{
    public void Configure(EntityTypeBuilder<PlanLimitedFeature> builder)
    {
        builder.ToTable("PlanLimitedFeatures");

        builder.HasKey(x => x.Id);

        builder.HasOne(pf => pf.LimitedFeature)
             .WithMany()
             .HasForeignKey(x => x.LimitedFeatureId);
    }
}