using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.RealTimeScheduling;

namespace Viora.Infrastructure.Configurations;

public class ScheduleCancellationConfiguration : IEntityTypeConfiguration<ScheduleCancellations>
{
    public void Configure(EntityTypeBuilder<ScheduleCancellations> builder)
    {
        builder.ToTable("ScheduleCancellations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ShiftId)
            .IsRequired();

        builder.Property(x => x.CancellationDate)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasOne<Shift>()
            .WithMany()
            .HasForeignKey(x => x.ShiftId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ShiftId);

        builder.HasIndex(x => x.CancellationDate);
    }
}
