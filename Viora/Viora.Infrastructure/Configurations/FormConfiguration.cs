using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using Viora.Domain.Forms;
using Viora.Domain.Services;
using Viora.Domain.Staffs;

namespace Viora.Infrastructure.Configurations;

internal class FormConfiguration : IEntityTypeConfiguration<Form>
{
    public void Configure(EntityTypeBuilder<Form> builder)
    {
        builder.ToTable("Forms");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.ServiceId)
            .IsRequired();

        builder.Property(f => f.StaffId)
            .IsRequired();

        builder.ComplexProperty(f => f.Name, nameBuilder =>
        {
            nameBuilder.Property(n => n.value)
                .HasColumnName("Name")
                .IsRequired();
        });

        var jsonConverter = new ValueConverter<JsonDocument, string>(
            v => v.RootElement.GetRawText(),
            v => JsonDocument.Parse(v)
            );

        builder.Property(f => f.Fields)
            .HasConversion(jsonConverter)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.HasOne<Service>()
            .WithOne()
            .HasForeignKey<Form>(f => f.ServiceId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Staff>()
            .WithMany()
            .HasForeignKey(f => f.StaffId);
    }
}
