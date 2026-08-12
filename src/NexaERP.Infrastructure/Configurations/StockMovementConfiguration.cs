using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaERP.Domain.Entities;

namespace NexaERP.Infrastructure.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(sm => sm.Id);

        // Performance indexes (defined in the migration — also documented in /database/README.md)
        builder.HasIndex(sm => new { sm.ProductId, sm.WarehouseId, sm.CreatedAt });

        builder.Property(sm => sm.MovementType).IsRequired();
        builder.Property(sm => sm.Quantity).IsRequired();

        builder.HasOne(sm => sm.Product)
               .WithMany(p => p.StockMovements)
               .HasForeignKey(sm => sm.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sm => sm.Warehouse)
               .WithMany(w => w.StockMovements)
               .HasForeignKey(sm => sm.WarehouseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sm => sm.CreatedBy)
               .WithMany()
               .HasForeignKey(sm => sm.CreatedById)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
