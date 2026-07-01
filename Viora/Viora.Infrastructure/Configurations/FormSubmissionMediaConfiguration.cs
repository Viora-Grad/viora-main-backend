using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Forms;
using Viora.Domain.Medias;

namespace Viora.Infrastructure.Configurations;

internal class FormSubmissionMediaConfiguration : IEntityTypeConfiguration<FormSubmissionMedia>
{
    public void Configure(EntityTypeBuilder<FormSubmissionMedia> builder)
    {
        builder.ToTable("FormSubmissionMedias");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.FormSubmissionId)
            .IsRequired();

        builder.Property(f => f.MediaId)
            .IsRequired();

        builder.HasOne<FormSubmission>()
            .WithMany()
            .HasForeignKey(f => f.FormSubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<MediaFile>()
            .WithMany()
            .HasForeignKey(f => f.MediaId);
    }
}
