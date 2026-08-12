using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaERP.Domain.Entities;

namespace NexaERP.Infrastructure.Configurations;

public class SalesOrderItemConfiguration : IEntityTypeConfiguration<SalesOrderItem>
{
    public void Configure(EntityTypeBuilder<SalesOrderItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");

        // Performance index on ProductId (for sales summary queries)
        builder.HasIndex(i => i.ProductId);

        builder.HasOne(i => i.SalesOrder)
               .WithMany(so => so.Items)
               .HasForeignKey(i => i.SalesOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        // RESTRICT on Product delete — preserve historical order data (prefer soft-delete via IsActive)
        builder.HasOne(i => i.Product)
               .WithMany(p => p.SalesOrderItems)
               .HasForeignKey(i => i.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
