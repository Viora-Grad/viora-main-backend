using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Branches;
using Viora.Domain.Medias;
using Viora.Domain.Services;
using Viora.Domain.Shared;

namespace Viora.Infrastructure.Configurations;

internal class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable(nameof(Service));

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.BranchId)
            .IsRequired();

        builder.Property(s => s.Type)
            .HasConversion(
                (ServiceType s) => s.Value,
                v => ServiceType.FromValue(v))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.Duration)
            .IsRequired();

        builder.ComplexProperty(s => s.Name, nb =>
        {
            nb.Property(n => n.Value)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.ComplexProperty(s => s.Description, db =>
        {
            db.Property(d => d.Value)
                .HasColumnName("Description")
                .HasMaxLength(500)
                .IsRequired();
        });

        builder.ComplexProperty(s => s.Cost, mb =>
        {
            mb.Property(m => m.Amount)
                .HasColumnName("CostAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            mb.ComplexProperty(m => m.Currency, cb =>
            {
                cb.Property(c => c.Code)
                    .HasColumnName("CostCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });
        });

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(b => b.Gallery);

        builder.HasMany<MediaFile>("_gallery")
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "ServiceGallery",
                r => r.HasOne<MediaFile>().WithMany().HasForeignKey("MediaFileId").OnDelete(DeleteBehavior.Restrict),
                l => l.HasOne<Service>().WithMany().HasForeignKey("ServiceId"),
                j =>
                {
                    j.ToTable("ServiceGallery");
                    j.HasIndex("ServiceId").HasDatabaseName("IX_ServiceGallery_ServiceId");
                    j.HasIndex("MediaFileId").HasDatabaseName("IX_ServiceGallery_MediaFileId");
                });
    }
}
