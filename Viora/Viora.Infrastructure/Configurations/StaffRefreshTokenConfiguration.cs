using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Infrastructure.Authentication;

namespace Viora.Infrastructure.Configurations;

internal class StaffRefreshTokenConfiguration : IEntityTypeConfiguration<StaffRefreshToken>
{
    public void Configure(EntityTypeBuilder<StaffRefreshToken> builder)
    {
        builder.ToTable("StaffRefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(256);


        builder.Property(rt => rt.Expires).IsRequired();
        builder.Property(rt => rt.CreationTime).IsRequired();
        builder.Property(rt => rt.StaffId).IsRequired();
        builder.Property(rt => rt.IsRevoked).IsRequired();

        builder.HasIndex(rt => rt.TokenHash).IsUnique();
        builder.HasIndex(rt => rt.StaffId)
            .IsUnique()
            .HasFilter("[IsRevoked] = 0"); // Ensure only one active refresh token per user
    }
}
