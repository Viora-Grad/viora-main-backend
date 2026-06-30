using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Prescriptions;

namespace Viora.Infrastructure.Configurations;

internal class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("PrescriptionItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PrescriptionId)
            .IsRequired();

        builder.ComplexProperty(x => x.Name, name =>
        {
            name.Property(n => n.Value)
                .HasColumnName("Name")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.ComplexProperty(x => x.Dosage, dosage =>
        {
            dosage.Property(d => d.Dose)
                .HasColumnName("Dose")
                .HasMaxLength(200)
                .IsRequired();

            dosage.Property(d => d.Frequency)
                .HasColumnName("Frequency")
                .IsRequired();
            dosage.Property(d => d.Duration)
                .HasColumnName("Duration")
                .IsRequired();
        });


        builder.ComplexProperty(x => x.Note, note =>
        {
            note.Property(n => n.Value)
                .HasColumnName("Note")
                .HasMaxLength(200)
                .IsRequired();
        });
    }
}

