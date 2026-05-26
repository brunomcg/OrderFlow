using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Data.Configurations;

public class SalesOrderItemConfiguration : IEntityTypeConfiguration<SalesOrderItem>
{
    public void Configure(EntityTypeBuilder<SalesOrderItem> builder)
    {
        // 💡 SOLUÇÃO: Centraliza o nome da tabela e as Check Constraints em uma única chamada
        builder.ToTable("sales_order_items", t =>
        {
            t.HasCheckConstraint("chk_quantity_positive", "quantity > 0");
            t.HasCheckConstraint("chk_price_positive", "unit_price >= 0");
        });

        // Define a Chave Primária Composta (OrderId + ProductId)
        builder.HasKey(oi => new { oi.SalesOrderId, oi.ProductId });

        builder.Property(oi => oi.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(oi => oi.Quantity)
            .IsRequired();

        // 💡 Ajuste de Relacionamento: Como o relacionamento já foi mapeado de forma 
        // completa em SalesOrderConfiguration, aqui nós apenas declaramos a chave estrangeira 
        // para o mapeamento ficar limpo e não duplicar a configuração da propriedade 'Items'.
        builder.HasOne(oi => oi.SalesOrder)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento: Um Item aponta para um Produto
        builder.HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}