using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viora.Domain.Branches;
using Viora.Domain.Inventory;
using Viora.Domain.Medias;

namespace Viora.Infrastructure.Configurations;

internal class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable(nameof(InventoryItem), t =>
        {
            t.HasCheckConstraint("CK_InventoryItem_Quantity_NonNegative", "[Quantity] >= 0");
            t.HasCheckConstraint("CK_InventoryItem_MinimumThreshold_NonNegative", "[MinimumThreshold] >= 0");
        });


        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.ItemImageId);

        builder.ComplexProperty(x => x.Name, b =>
        {
            b.Property(n => n.Value)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.OwnsOne(x => x.Notes, b =>
        {
            b.Property(n => n.Value)
                .HasColumnName("Notes")
                .HasMaxLength(500);
        });

        builder.ComplexProperty(x => x.Quantity, b =>
        {
            b.Property(q => q.Value)
                .HasColumnName("Quantity")
                .IsRequired();
        });

        builder.ComplexProperty(x => x.MinimumThreshold, b =>
        {
            b.Property(m => m.Value)
                .HasColumnName("MinimumThreshold")
                .IsRequired();
        });

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(item => item.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<MediaFile>()
            .WithMany()
            .HasForeignKey(item => item.ItemImageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.BranchId);
    }
}
