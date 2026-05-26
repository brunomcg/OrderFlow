using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // 💡 SOLUÇÃO: Centraliza o nome da tabela e as Check Constraints em uma única chamada
        builder.ToTable("product", t =>
        {
            t.HasCheckConstraint("chk_product_price_positive", "unit_price >= 0");
            t.HasCheckConstraint("chk_product_stock_positive", "available_quantity >= 0");
        });

        // Chave primária mapeada como BIGINT auto-incremental
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Name)
            .HasMaxLength(150)
            .IsRequired();

        // Configuração de precisão decimal para o preço
        builder.Property(p => p.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.AvailableQuantity)
            .IsRequired();
    }
}