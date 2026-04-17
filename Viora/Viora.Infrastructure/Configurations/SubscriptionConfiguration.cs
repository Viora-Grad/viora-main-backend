using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Organizations;
using Viora.Domain.Plans;
using Viora.Domain.Subscriptions;

namespace Viora.Infrastructure.Configurations;

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");


        builder.HasKey(x => x.Id);


        builder.Property(x => x.PlanId)
            .IsRequired();

        builder.HasOne<Plan>()
            .WithMany()
            .HasForeignKey(x => x.PlanId);

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.SubscriptionsStartTime)
            .IsRequired();

        builder.Property(x => x.SubscriptionsEndTime)
            .IsRequired();


    }
}
