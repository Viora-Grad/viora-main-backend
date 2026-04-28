using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Users.Identity;

namespace Viora.Infrastructure.Configurations;

internal class AuthIdentityConfiguration : IEntityTypeConfiguration<AuthIdentity>
{
    public void Configure(EntityTypeBuilder<AuthIdentity> builder)
    {
        builder.ToTable("AuthIdentities");
        builder.HasKey(ai => ai.Id);

        builder.Property(ai => ai.CreatedAt)
            .IsRequired();
        builder.Property(ai => ai.LastLoginAt)
            .IsRequired(false);

        builder.Property(ai => ai.Provider)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(ai => ai.ProviderKey)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(ai => new { ai.Provider, ai.ProviderKey })
            .IsUnique();

        builder.Property(ai => ai.UserId).IsRequired();
        builder.HasOne(ai => ai.User)
            .WithMany(user => user.Identities)
            .HasForeignKey(ai => ai.UserId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
