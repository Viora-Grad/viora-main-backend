using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Notifications;
using Viora.Domain.Notifications.Internal;
using Viora.Domain.Users.Identity;

namespace Viora.Infrastructure.Configurations;

internal class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.RecipientId)
            .IsRequired();

        builder.Property(n => n.Title)
            .IsRequired()
            .HasConversion(
                title => title.Value,
                value => new Title(value));

        builder.Property(n => n.Body)
            .IsRequired()
            .HasConversion(
                body => body.Value,
                value => new Body(value));

        builder.Property(n => n.SentAt)
            .IsRequired();

        builder.Property(n => n.IsRead)
            .IsRequired();

        builder.HasIndex(n => n.RecipientId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(n => n.RecipientId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
