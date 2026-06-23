using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.ChatSessions;

namespace Viora.Infrastructure.Configurations;

internal sealed class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
{
    public void Configure(EntityTypeBuilder<ChatSession> builder)
    {
        builder.ToTable("ChatSessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(s => s.LastActiveAt)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(s => s.Title)
            .HasMaxLength(80);

        builder.Property(s => s.HistoryJson)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(s => s.UserId);
        builder.HasIndex(s => s.LastActiveAt);
    }
}
// EF picks this up automatically via modelBuilder.ApplyConfigurationsFromAssembly()
// which is already in your ApplicationDbContext.OnModelCreating — nothing else needed.