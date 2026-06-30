using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Staffs;

namespace Viora.Infrastructure.Configurations;

internal class StaffInvitationTokenConfiguration : IEntityTypeConfiguration<StaffToken>
{
    public void Configure(EntityTypeBuilder<StaffToken> builder)
    {
        builder.ToTable("StaffInvitationTokens");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.StaffId)
            .IsRequired();

        builder.HasOne(t => t.Staff)
            .WithOne()
            .HasForeignKey<StaffToken>(t => t.StaffId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Property(t => t.TokenHash)
            .IsRequired();

        builder.Property(t => t.Expiration)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.RevokedAt)
            .IsRequired(false);

        builder.Property(t => t.UsedAt)
            .IsRequired(false);

        builder.HasIndex(t => t.StaffId).IsUnique();
        builder.HasIndex(t => t.TokenHash)
            .IsUnique();

        builder.HasQueryFilter(t => t.RevokedAt == null && t.UsedAt == null);
    }
}
