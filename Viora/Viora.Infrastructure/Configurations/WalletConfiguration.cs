using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.WalletTransactions;
using Viora.Domain.Wallets;

namespace Viora.Infrastructure.Configurations;

internal sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("Wallets");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).ValueGeneratedNever();

        // Derived, not persisted.
        builder.Ignore(w => w.Type);
        builder.Ignore(w => w.OwnerId);

        builder.Property(w => w.UserId);
        builder.Property(w => w.BranchId);
        builder.Property(w => w.OpenedAtUtc).IsRequired();

        builder.ComplexProperty(w => w.Currency, c =>
        {
            c.Property(x => x.Code)
                .HasColumnName("CurrencyCode")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.ComplexProperty(w => w.Balance, b =>
        {
            b.Property(m => m.Amount)
                .HasColumnName("BalanceAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            b.ComplexProperty(m => m.Currency, cb =>
            {
                cb.Property(c => c.Code)
                    .HasColumnName("BalanceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });
        });

        // Append-only ledger; loaded explicitly, never for balance computation.
        builder.HasMany(w => w.Transactions)
            .WithOne()
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(w => w.Transactions)
            .HasField("_transactions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // One wallet per owner (customer or branch).
        builder.HasIndex(w => w.UserId)
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL");

        builder.HasIndex(w => w.BranchId)
            .IsUnique()
            .HasFilter("[BranchId] IS NOT NULL");
    }
}
