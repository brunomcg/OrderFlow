using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Data.Configurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("sales_order", t =>
        {
            t.HasCheckConstraint("chk_sales_order_total_positive", "total >= 0");
        });

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .ValueGeneratedOnAdd();

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.Total)
            .HasPrecision(18, 2)
            .IsRequired();

        var navigation = builder.Metadata.FindNavigation(nameof(SalesOrder.Items));
        navigation?.SetPropertyAccessMode(PropertyAccessMode.Field);


        builder.HasMany(o => o.Items)
            .WithOne(i => i.SalesOrder)
            .HasForeignKey(i => i.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Currency)
            .WithMany()
            .HasForeignKey(o => o.CurrencyCode) // Aponta para a propriedade string física da classe
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.SalesOrderStatus)
            .WithMany()
            .HasForeignKey(o => o.SalesOrderStatusId) // Aponta para a propriedade INT física da classe
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
