using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Plans;

namespace Viora.Infrastructure.Configurations;

internal sealed class PlanFeatureConfiguration : IEntityTypeConfiguration<PlanFeature>
{
    public void Configure(EntityTypeBuilder<PlanFeature> builder)
    {
        builder.ToTable("PlanFeatures");

        builder.HasKey(x => x.Id);

        builder.HasOne(pf => pf.Feature)
             .WithMany()
             .HasForeignKey(x => x.FeatureId);


    }
}
