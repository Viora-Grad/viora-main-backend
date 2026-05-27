using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Organizations.OnBoardings;
using Viora.Domain.Shared;
using Viora.Domain.Users.Owners;

namespace Viora.Infrastructure.Configurations;

internal sealed class OrganizationApplicationConfiguration : IEntityTypeConfiguration<OrganizationApplication>
{
    public void Configure(EntityTypeBuilder<OrganizationApplication> builder)
    {
        builder.ToTable("OrganizationApplications");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.OwnerId)
            .IsRequired();

        builder.Property(a => a.CountryId)
            .IsRequired();

        builder.ComplexProperty(a => a.ProposedName, name =>
        {
            name.Property(n => n.Value)
                .HasColumnName("ProposedName")
                .HasMaxLength(255)
                .IsRequired();
        });

        builder.ComplexProperty(a => a.ApplicationLetter, letter =>
        {
            letter.Property(l => l.Value)
                .HasColumnName("ApplicationLetter")
                .HasMaxLength(5000)
                .IsRequired();
        });

        builder.ComplexProperty(a => a.ServiceDescription, desc =>
        {
            desc.Property(d => d.Value)
                .HasColumnName("ServiceDescription")
                .HasMaxLength(2000)
                .IsRequired();
        });

        builder.ComplexProperty(a => a.BillingEmail, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("BillingEmail")
                .HasMaxLength(320)
                .IsRequired();
        });

        builder.ComplexProperty(a => a.SupportEmail, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("SupportEmail")
                .HasMaxLength(320)
                .IsRequired();
        });

        builder.ComplexProperty(a => a.About, about =>
        {
            about.Property(a => a.Value)
                .HasColumnName("About")
                .HasMaxLength(500)
                .IsRequired();
        });

        builder.PrimitiveCollection(a => a.ProposedServicesType)
            .HasColumnName("ProposedServicesType")
            .ElementType(b => b.HasConversion<string>().HasMaxLength(50));

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.ReferralSource)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.SubmittedOnUtc)
            .IsRequired();

        builder.Property(a => a.ExpiryDateUtc)
            .IsRequired();

        builder.Property(a => a.RejectedBy);


        builder.HasOne<Owner>()
            .WithOne()
            .HasForeignKey<OrganizationApplication>(o => o.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(a => a.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.OwnerId).IsUnique();
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.CountryId);
    }
}