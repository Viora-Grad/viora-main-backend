using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Marketing;

namespace Viora.Infrastructure.Configurations;

internal sealed class MarketingChatMessageConfiguration : IEntityTypeConfiguration<MarketingChatMessage>
{
    public void Configure(EntityTypeBuilder<MarketingChatMessage> builder)
    {
        builder.ToTable("MarketingChatMessages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.SessionId).IsRequired();

        builder.Property(m => m.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.Source)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Post/idea text can be long; default nvarchar(max).
        builder.Property(m => m.Content).IsRequired();

        // Nullable enum stored as string for auditing which intent was assigned to a user turn.
        builder.Property(m => m.DetectedIntent)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(m => m.CreatedAtUtc).IsRequired();

        builder.HasIndex(m => m.SessionId);
    }
}
