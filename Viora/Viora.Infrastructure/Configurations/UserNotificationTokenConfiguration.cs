namespace Viora.Infrastructure.Configurations;

/*internal class UserNotificationTokenConfiguration : IEntityTypeConfiguration<UserNotificationToken>
{
    public void Configure(EntityTypeBuilder<UserNotificationToken> builder)
    {
        builder.ToTable("UserNotificationTokens");

        builder.HasKey(x => x.UserId);

        builder.Property(x => x.DeviceToken)
            .IsRequired()
            .HasMaxLength(255);
    }
}*/
