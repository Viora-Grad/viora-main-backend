using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Organizations.OrganizationHistory;

namespace Viora.Infrastructure.Configurations;

internal class OrganizationVisitConfiguration : IEntityTypeConfiguration<OrganizationVisits>
{
    public void Configure(EntityTypeBuilder<OrganizationVisits> builder)
    {
        builder.ToTable("OrganizationVisits");

        builder.HasKey(ov => ov.Id);

        builder.Property(ov => ov.OrganizationId)
            .IsRequired();

        builder.Property(ov => ov.CustomerId)
            .IsRequired();

        builder.Property(ov => ov.VisitedAt)
            .IsRequired();

        builder.HasOne(ov => ov.Organization)
            .WithMany()
            .HasForeignKey(ov => ov.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ov => ov.Customer)
            .WithMany()
            .HasForeignKey(ov => ov.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
