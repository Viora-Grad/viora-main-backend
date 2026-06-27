using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.RealTimeScheduling;

namespace Viora.Infrastructure.Configurations;

internal class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("Shifts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ScheduleId)
            .IsRequired();

        builder.Property(x => x.StaffId)
            .IsRequired();

        builder.Property(x => x.StartTime)
            .HasColumnType("time")
            .IsRequired();

        builder.Property(x => x.EndTime)
            .HasColumnType("time")
            .IsRequired();

        builder.HasIndex(x => x.StaffId);

        builder.HasIndex(x => new
        {
            x.ScheduleId,
            x.StaffId
        });
    }
}
