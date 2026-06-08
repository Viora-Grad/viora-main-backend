using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Medias;
using Viora.Domain.Medias.Internals;
using Viora.Domain.Organizations.OrganizationDetails;

namespace Viora.Infrastructure.Configurations;

internal class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.ToTable("MediaFiles");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        builder.Property(m => m.Name)
             .HasConversion(
                v => v.Value,
                v => new Name(v))
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(m => m.MimeType)
             .HasConversion(
                v => v.Value,
                v => new MimeType(v))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.Key)
            .HasConversion(
                v => v.Value,
                v => new MediaKey(v))
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(m => m.SizeInBytes)
            .IsRequired();

        builder.Property(m => m.UploadedAtUtc)
            .IsRequired();

        builder.Ignore(m => m.CategoryType);

        builder.HasOne<Organization>()
            .WithMany()
            .HasForeignKey(m => m.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(m => m.Key)
            .IsUnique();

        builder.HasIndex(m => m.UploadedAtUtc);
        builder.HasIndex(m => m.OrganizationId);
    }
}
