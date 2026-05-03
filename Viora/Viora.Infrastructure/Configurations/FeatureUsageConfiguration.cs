using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Organizations;
using Viora.Domain.Plans.Features;

namespace Viora.Infrastructure.Configurations;

internal sealed class FeatureUsageConfiguration : IEntityTypeConfiguration<FeatureUsage>
{
    public void Configure(EntityTypeBuilder<FeatureUsage> builder)
    {
        builder.ToTable("FeatureUsages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganizationId)
            .IsRequired();
        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(fu => fu.OrganizationId);

        builder.Property(x => x.Quota)
            .IsRequired();

        builder.Property(x => x.PeriodStart)
            .IsRequired();

        builder.Property(x => x.PeriodEnd)
            .IsRequired();

    }
}
