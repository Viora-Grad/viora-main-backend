using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Forms;

namespace Viora.Infrastructure.Configurations;

internal class FormSubmissionConfiguration : IEntityTypeConfiguration<FormSubmission>
{
    public void Configure(EntityTypeBuilder<FormSubmission> builder)
    {
        builder.ToTable("FormSubmissions");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.AppointmentId)
            .IsRequired();

        builder.Property(f => f.FormId)
            .IsRequired();

        builder.Property(f => f.Submission)
            .HasColumnType("nvarchar(max)") // SQL Server
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        builder.HasOne<Form>()
            .WithMany()
            .HasForeignKey(f => f.FormId)
            .OnDelete(DeleteBehavior.Cascade);

        //builder.HasOne<Appointment>()
        //    .WithMany()
        //    .HasForeignKey(f => f.AppointmentId)
        //    .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => f.AppointmentId);

        builder.HasIndex(f => f.FormId);
    }
}
