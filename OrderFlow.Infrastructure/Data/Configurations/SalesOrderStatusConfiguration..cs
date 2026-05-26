using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Data.Mappings;

public class OrderStatusConfiguration : IEntityTypeConfiguration<SalesOrderStatus>
{
    public void Configure(EntityTypeBuilder<SalesOrderStatus> builder)
    {
        builder.ToTable("sales_order_status");

        builder.HasKey(os => os.Id);

        builder.Property(os => os.Id)
            .ValueGeneratedNever();

        builder.Property(os => os.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(os => os.Description)
            .HasMaxLength(250)
            .IsRequired();

        builder.HasData(SalesOrderStatus.List());
    }
}
