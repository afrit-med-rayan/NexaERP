using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaERP.Domain.Entities;

namespace NexaERP.Infrastructure.Configurations;

public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.HasKey(i => i.Id);

        // Unique per (Product, Warehouse)
        builder.HasIndex(i => new { i.ProductId, i.WarehouseId }).IsUnique();

        // Database-level safety net — application layer also checks before decrement
        builder.ToTable(t => t.HasCheckConstraint("CK_Inventory_Quantity_NonNegative", "[Quantity] >= 0"));

        builder.HasOne(i => i.Product)
               .WithMany(p => p.Inventories)
               .HasForeignKey(i => i.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Warehouse)
               .WithMany(w => w.Inventories)
               .HasForeignKey(i => i.WarehouseId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
