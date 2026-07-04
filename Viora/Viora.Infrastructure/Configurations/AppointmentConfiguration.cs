using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Appointments;

namespace Viora.Infrastructure.Configurations;
/// <summary>
/// Service, Staff, and Payment relationships are configured in their respective configurations becuase they are not implemented yet 
/// </summary>
internal class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.CustomerId)
            .IsRequired(false);

        builder.HasOne(a => a.Customer)
            .WithMany()
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.ServiceId)
            .IsRequired();
        builder.HasOne(a => a.Service)
            .WithMany()
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.StaffId)
            .IsRequired();
        builder.HasOne(a => a.Staff)
            .WithMany()
            .HasForeignKey(a => a.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.BranchId)
            .IsRequired();
        builder.HasOne(a => a.Branch)
            .WithMany()
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.AppointmentQueueNumber)
            .IsRequired();

        builder.Property(a => a.PaymentId);

        builder.Property(a => a.ReservationDate)
            .IsRequired();

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(a => a.PayMethod)
            .IsRequired();

        builder.Property(a => a.IsCheckedIn)
            .IsRequired();

        builder.Property(a => a.CreatedBy)
            .IsRequired();

        builder.Property(a => a.RequestPlatform)
            .IsRequired();

        builder.Property(a => a.EstimatedDurationMinutes)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.LastUpdatedAt);

        builder.Property<byte[]>("RowVersion")
               .IsRowVersion()
               .IsConcurrencyToken()
               .HasColumnName("RowVersion");

        builder.HasIndex(a => a.CustomerId);
        builder.HasIndex(a => a.ServiceId);
        builder.HasIndex(a => a.StaffId);
    }
}
