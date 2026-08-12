using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaERP.Domain.Entities;

namespace NexaERP.Infrastructure.Configurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.HasKey(so => so.Id);
        builder.Property(so => so.Status).IsRequired();

        builder.HasOne(so => so.Customer)
               .WithMany(c => c.SalesOrders)
               .HasForeignKey(so => so.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(so => so.CreatedBy)
               .WithMany()
               .HasForeignKey(so => so.CreatedById)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
