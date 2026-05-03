using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Subscriptions;

namespace Viora.Infrastructure.Configurations;

internal sealed class SubscriptionAddonConfiguration : IEntityTypeConfiguration<SubscriptionAddon>
{
    public void Configure(EntityTypeBuilder<SubscriptionAddon> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SubscriptionId)
            .IsRequired();

        builder.Property(x => x.LimitedFeatureAddonId)
            .IsRequired();

        builder.HasOne(sa => sa.LimitedFeatureAddon)
            .WithMany()
            .HasForeignKey(sa => sa.LimitedFeatureAddonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
