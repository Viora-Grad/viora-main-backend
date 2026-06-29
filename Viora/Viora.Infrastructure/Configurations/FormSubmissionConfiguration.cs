using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using Viora.Domain.Appointments;
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
        var jsonConverter = new ValueConverter<JsonDocument, string>(
            v => v.RootElement.GetRawText(),
            v => JsonDocument.Parse(v));

        builder.Property(f => f.Submission)
            .HasConversion(jsonConverter)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        builder.HasOne<Form>()
            .WithMany()
            .HasForeignKey(f => f.FormId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Appointment>()
            .WithOne()
            .HasForeignKey<FormSubmission>(fs => fs.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => f.AppointmentId);

        builder.HasIndex(f => f.FormId);
    }
}
