using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Inventory;
using Viora.Domain.InventoryMovements;

namespace Viora.Infrastructure.Configurations;

internal class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.ToTable(nameof(InventoryMovement), t =>
        {
            t.HasCheckConstraint("CK_InventoryMovement_Quantity_NonNegative", "[Quantity] >= 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.InventoryItemId)
            .IsRequired();

        builder.Property(x => x.PerformedByUserId)
            .IsRequired();

        builder.Property(x => x.MovementType)
            .HasConversion<string>();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.HasOne<InventoryItem>()
            .WithMany()
            .HasForeignKey(x => x.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // TODO add the staff relation here
    }
}
