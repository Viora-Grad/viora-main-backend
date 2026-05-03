using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Users.Identity;
using Viora.Infrastructure.Authentication;

namespace Viora.Infrastructure.Configurations;

internal class LocalCredentialConfiguration : IEntityTypeConfiguration<LocalCredential>
{
    public void Configure(EntityTypeBuilder<LocalCredential> builder)
    {
        builder.HasKey(lc => lc.UserId);
        builder.Property(lc => lc.HashedPassword).IsRequired();
        builder.Property(lc => lc.FailedLoginAttempts).HasDefaultValue(0);
        builder.Property(lc => lc.HashVersion).HasDefaultValue(1);
        builder.Property(lc => lc.LastChangedAt);

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<LocalCredential>(lc => lc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
