using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.Suspensions;
using Viora.Domain.Users.Owners;

namespace Viora.Infrastructure.Configurations;

public class SuspensionConfiguration : IEntityTypeConfiguration<Suspension>
{
    public void Configure(EntityTypeBuilder<Suspension> builder)
    {
        builder.ToTable("Suspensions");


        builder.HasKey(s => s.Id);

        builder.Property(s => s.OwnerId)
            .IsRequired();

        builder.Property(s => s.OrganizationId);

        builder.Property(s => s.SuspendedById);

        builder.ComplexProperty(s => s.OrganizationName, name =>
        {
            name.Property(n => n.Value)
                .HasColumnName("OrganizationName")
                .HasMaxLength(50)
                .IsRequired();
        });

        builder.Property(s => s.Reason)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Source)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.ComplexProperty(s => s.Notes, notes =>
        {
            notes.Property(n => n.Value)
                .HasColumnName("Notes")
                .HasMaxLength(1000)
                .IsRequired();
        });

        builder.HasOne<Owner>()
            .WithMany()
            .HasForeignKey(s => s.OwnerId);

        builder.HasOne<Organization>()
            .WithOne()
            .HasForeignKey<Suspension>(s => s.OrganizationId);

        builder.Property(s => s.Reason)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(s => s.OrganizationId)
            .IsUnique();

        builder.HasIndex(s => s.OwnerId);
    }
}
