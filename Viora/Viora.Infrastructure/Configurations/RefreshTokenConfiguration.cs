using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Infrastructure.Authentication;

namespace Viora.Infrastructure.Configurations;

internal class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(rt => rt.Expires).IsRequired();
        builder.Property(rt => rt.CreationTime).IsRequired();
        builder.Property(rt => rt.UserId).IsRequired();
        builder.Property(rt => rt.IsRevoked).IsRequired();

        builder.HasIndex(rt => rt.TokenHash).IsUnique();

        builder.HasIndex(rt => rt.UserId);

        builder.HasQueryFilter(rt => !rt.IsRevoked);
    }
}
