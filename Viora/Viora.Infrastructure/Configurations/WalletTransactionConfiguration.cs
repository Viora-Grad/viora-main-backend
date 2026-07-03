using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.WalletTransactions;
using Viora.Domain.WalletTransactions.Internals;

namespace Viora.Infrastructure.Configurations;

internal sealed class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        builder.ToTable("WalletTransactions");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        // Derived, not persisted.
        builder.Ignore(t => t.EffectiveAmount);

        builder.Property(t => t.WalletId).IsRequired();
        builder.Property(t => t.CreatedAtUtc).IsRequired();

        builder.Property(t => t.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Purpose)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasConversion(d => d.Value, v => new Description(v))
            .HasColumnName("Description")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.ReferenceId)
            .HasConversion(r => r.Value, v => new ExternalReferenceId(v))
            .HasColumnName("ReferenceId")
            .HasMaxLength(200)
            .IsRequired();

        builder.ComplexProperty(t => t.Money, m =>
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

        builder.ComplexProperty(t => t.RunningBalance, m =>
        {
            m.Property(x => x.Amount)
                .HasColumnName("RunningBalanceAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            m.ComplexProperty(x => x.Currency, cb =>
            {
                cb.Property(c => c.Code)
                    .HasColumnName("RunningBalanceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });
        });

        builder.HasIndex(t => t.WalletId);

        // Idempotency guard: a given (Type, Purpose, ReferenceId) can only be written once, so a replayed
        // recharge webhook / settlement / refund event cannot double-credit. Filtered to skip empty refs.
        builder.HasIndex(t => new { t.Type, t.Purpose, t.ReferenceId })
            .IsUnique()
            .HasFilter("[ReferenceId] <> ''");
    }
}
