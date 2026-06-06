using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Medias;
using Viora.Domain.Organizations.LegalPapers;

namespace Viora.Infrastructure.Configurations;

internal sealed class LegalPaperConfiguration : IEntityTypeConfiguration<LegalPaper>
{
    public void Configure(EntityTypeBuilder<LegalPaper> builder)
    {
        builder.ToTable("LegalPapers");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.AttachmentId)
            .IsRequired();

        builder.Property(p => p.ApprovedById);

        builder.ComplexProperty(p => p.Name, name =>
        {
            name.Property(n => n.Value)
                .HasColumnName("Name")
                .HasMaxLength(255)
                .IsRequired();
        });

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.SubmissionDateUtc)
            .IsRequired();

        builder.Property(p => p.ExpiryDateUtc);

        builder.HasOne<MediaFile>()
            .WithOne()
            .HasForeignKey<LegalPaper>(p => p.AttachmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.AttachmentId).IsUnique();
        builder.HasIndex(p => p.Status);
    }
}