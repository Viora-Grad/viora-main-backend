using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

        builder.Property(f => f.Fields)
            .HasColumnType("nvarchar(max)") // SQL Server
            .IsRequired();


        builder.HasOne<Service>()
            .WithOne()
            .HasForeignKey<Form>(f => f.ServiceId);

        builder.HasOne<Staff>()
            .WithMany()
            .HasForeignKey(f => f.StaffId);
    }
}
