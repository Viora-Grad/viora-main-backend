using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Users.Identity;
using Viora.Infrastructure.NotificationService;

namespace Viora.Infrastructure.Configurations;

internal class UserNotificationTokenConfiguration : IEntityTypeConfiguration<UserNotificationToken>
{
    public void Configure(EntityTypeBuilder<UserNotificationToken> builder)
    {
        builder.ToTable("UserNotificationTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.IsRevoked)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.DeviceToken)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(x => x.DeviceToken)
            .IsUnique();

        builder.HasIndex(x => x.UserId);

        builder.HasQueryFilter(x => !x.IsRevoked);

    }
}
