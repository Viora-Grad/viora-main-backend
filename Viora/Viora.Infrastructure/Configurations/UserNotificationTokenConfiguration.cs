namespace Viora.Infrastructure.Configurations;
/* commented out because the notification service is not implemented yet, and this configuration is not used anywhere in the codebase. It can be uncommented and used when the notification service is implemented.
internal class UserNotificationTokenConfiguration : IEntityTypeConfiguration<UserNotificationToken>
{
    public void Configure(EntityTypeBuilder<UserNotificationToken> builder)
    {
        builder.ToTable("UserNotificationTokens");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.DeviceToken)
            .IsRequired()
            .HasMaxLength(255);
    }
}
*/