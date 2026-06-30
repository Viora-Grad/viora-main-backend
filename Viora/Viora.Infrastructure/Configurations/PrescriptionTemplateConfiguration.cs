using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Prescriptions;

namespace Viora.Infrastructure.Configurations;

internal class PrescriptionTemplateConfiguration : IEntityTypeConfiguration<PrescriptionTemplate>
{
    public void Configure(EntityTypeBuilder<PrescriptionTemplate> builder)
    {
        builder.ToTable("PrescriptionTemplates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrganizationId)
            .IsRequired();

        builder.ComplexProperty(x => x.Name, name =>
        {
            name.Property(n => n.Value)
                .HasColumnName("Name")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.Property(x => x.TemplateMediaId)
            .IsRequired();

        builder.Property(x => x.TopMargin)
            .IsRequired();

        builder.Property(x => x.LeftMargin)
            .IsRequired();

        builder.Property(x => x.RightMargin)
            .IsRequired();

        builder.Property(x => x.BottomMargin)
            .IsRequired();

        builder.HasOne(x => x.File)
            .WithOne()
            .HasForeignKey<PrescriptionTemplate>(x => x.TemplateMediaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OrganizationId);
    }
}
