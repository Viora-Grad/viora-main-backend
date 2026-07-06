using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Marketing;

namespace Viora.Infrastructure.Configurations;

internal sealed class MarketingChatSessionConfiguration : IEntityTypeConfiguration<MarketingChatSession>
{
    public void Configure(EntityTypeBuilder<MarketingChatSession> builder)
    {
        builder.ToTable("MarketingChatSessions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.OrganizationId).IsRequired();
        builder.Property(s => s.UserId).IsRequired();
        builder.Property(s => s.Title).HasMaxLength(200);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Free text snapshots; default nvarchar(max).
        builder.Property(s => s.LatestManusIdea);
        builder.Property(s => s.LatestImageUrl).HasMaxLength(1000);
        builder.Property(s => s.PendingManusTaskId).HasMaxLength(100);
        builder.Property(s => s.PendingManusTaskUrl).HasMaxLength(500);
        builder.Property(s => s.PostMessage);
        builder.Property(s => s.PostLink).HasMaxLength(2048);
        builder.Property(s => s.FacebookPostId).HasMaxLength(200);

        builder.Property(s => s.CreatedAtUtc).IsRequired();
        builder.Property(s => s.UpdatedAtUtc).IsRequired();

        builder.HasMany(s => s.Messages)
            .WithOne()
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Messages)
            .HasField("_messages")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(s => s.OrganizationId);
    }
}
