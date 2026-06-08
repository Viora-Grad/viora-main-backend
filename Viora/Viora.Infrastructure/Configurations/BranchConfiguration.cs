using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Viora.Domain.Branches;
using Viora.Domain.Branches.Internals;
using Viora.Domain.Medias;
using Viora.Domain.Organizations.OrganizationDetails;
using Viora.Domain.Shared;
using Viora.Domain.Shared.Internal;

namespace Viora.Infrastructure.Configurations;

internal class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable(nameof(Branch));

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        builder.Property(b => b.OrganizationId)
            .IsRequired();

        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.ComplexProperty(b => b.TimeZone, timeZone =>
        {
            timeZone.Property(t => t.Value)
            .HasMaxLength(100)
            .IsRequired();
        });

        builder.Property(b => b.Location)
            .HasColumnType("geography")
            .IsRequired();

        builder.ComplexProperty(b => b.Address, ab =>
        {
            ab.Property(a => a.Number).HasColumnName("AddressNumber").IsRequired();
            ab.Property(a => a.Street).HasColumnName("AddressStreet").HasMaxLength(200).IsRequired();
            ab.Property(a => a.City).HasColumnName("AddressCity").HasMaxLength(100).IsRequired();
            ab.Property(a => a.State).HasColumnName("AddressState").HasMaxLength(100).IsRequired();
            ab.Property(a => a.CountryId).HasColumnName("AddressCountryId").IsRequired();
            ab.Property(a => a.PostalCode).HasColumnName("AddressPostalCode").IsRequired();
        });

        builder.ComplexProperty(b => b.ContactEmail, eb =>
        {
            eb.Property(e => e.Value)
                .HasColumnName("ContactEmail")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.PrimitiveCollection("_services")
            .HasColumnName("Services")
            .ElementType(e => e
                .HasConversion(new ValueConverter<ServiceType, string>(
                    s => s.Value,
                    v => ServiceType.FromValue(v)))
                .HasMaxLength(100));

        // private properties are accessed through reflection not expressions thus the string names

        builder.Ignore(b => b.PhoneNumbers);

        builder.OwnsMany<PhoneNumber>("_phoneNumbers", pb =>
        {
            pb.ToTable("BranchPhoneNumber");
            pb.WithOwner().HasForeignKey("BranchId");
            pb.Property(p => p.Value)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(20)
                .IsRequired();
        });

        builder.Ignore(b => b.BusinessHours);

        builder.OwnsMany<BusinessHour>("_businessHours", bh =>
        {
            bh.ToTable("BranchBusinessHour");
            bh.WithOwner().HasForeignKey("BranchId");
            bh.HasKey("BranchId", nameof(BusinessHour.Day));
            bh.HasIndex("BranchId");
            bh.Property(b => b.Day).IsRequired();
            bh.Property(b => b.OpenTime).IsRequired();
            bh.Property(b => b.CloseTime).IsRequired();
        });

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(b => b.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.Gallery);

        builder.HasMany<MediaFile>("_gallery")
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "BranchGallery",
                r => r.HasOne<MediaFile>().WithMany().HasForeignKey("MediaFileId").OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne<Branch>().WithMany().HasForeignKey("BranchId"),
                j =>
                {
                    j.ToTable("BranchGallery");
                    j.HasIndex("BranchId").HasDatabaseName("IX_BranchGallery_BranchId");
                    j.HasIndex("MediaFileId").HasDatabaseName("IX_BranchGallery_MediaFileId");
                });
    }
}
