using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Viora.Domain.Medias;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Organizations.OrganizationDetails.Internal;
using Viora.Domain.Shared;

namespace Viora.Infrastructure.Configurations;

internal sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedNever();

        builder.Property(o => o.OwnerId)
            .IsRequired();

        builder.Property(o => o.CountryId)
            .IsRequired();

        builder.Property(o => o.LogoId);

        builder.Property(o => o.Name)
            .HasConversion(
                v => v.Value,
                v => new Name(v))
            .HasColumnName("Name")
            .HasMaxLength(255)
            .IsRequired();

        builder.ComplexProperty(o => o.About, about =>
        {
            about.Property(a => a.Value)
                .HasColumnName("About")
                .HasMaxLength(500)
                .IsRequired();
        });

        builder.ComplexProperty(o => o.ServiceDescription, desc =>
        {
            desc.Property(d => d.Value)
                .HasColumnName("ServiceDescription")
                .HasMaxLength(2000)
                .IsRequired();
        });

        builder.PrimitiveCollection(o => o.ServicesProvided)
            .HasColumnName("ServicesProvided")
            .ElementType(e => e
                .HasConversion(new ValueConverter<ServiceType, string>(
                    s => s.Value,
                    v => ServiceType.FromValue(v)))
                .HasMaxLength(100));

        builder.ComplexProperty(o => o.Rating, rating =>
        {
            rating.Property(r => r.Count)
                .HasColumnName("RatingCount")
                .IsRequired();

            rating.Property(r => r.AverageOutOfTen)
                .HasColumnName("RatingAverage")
                .HasColumnType("decimal(3,1)")
                .IsRequired();
        });

        builder.ComplexProperty(o => o.BillingEmail, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("BillingEmail")
                .HasMaxLength(320)
                .IsRequired();
        });

        builder.ComplexProperty(o => o.SupportEmail, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("SupportEmail")
                .HasMaxLength(320)
                .IsRequired();
        });

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(o => o.ReferralSource)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(o => o.JoinedOnUtc)
            .IsRequired();



        builder.HasOne<MediaFile>()
            .WithMany()
            .HasForeignKey(o => o.LogoId)
            .OnDelete(DeleteBehavior.SetNull);



        builder.HasIndex(o => o.OwnerId)
            .IsUnique();

        builder.HasIndex(o => o.CountryId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.Name);    // for names violation
    }
}