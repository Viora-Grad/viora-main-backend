using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.WalletPromises;

namespace Viora.Infrastructure.Configurations;

internal sealed class WalletPromiseConfiguration : IEntityTypeConfiguration<WalletPromise>
{
    public void Configure(EntityTypeBuilder<WalletPromise> builder)
    {
        builder.ToTable("WalletPromises");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.FromWalletId).IsRequired();
        builder.Property(p => p.ToWalletId).IsRequired();
        builder.Property(p => p.SourceTransactionId).IsRequired();
        builder.Property(p => p.DestinationTransactionId);
        builder.Property(p => p.ScheduledEventId);
        builder.Property(p => p.ExpiresAtUtc).IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.ComplexProperty(p => p.Money, m =>
        {
            m.Property(x => x.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            m.ComplexProperty(x => x.Currency, cb =>
            {
                cb.Property(c => c.Code)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });
        });

        builder.HasIndex(p => p.SourceTransactionId).IsUnique();
        builder.HasIndex(p => p.FromWalletId);
    }
}
