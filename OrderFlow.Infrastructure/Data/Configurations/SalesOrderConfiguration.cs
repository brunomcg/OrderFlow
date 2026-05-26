using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Data.Configurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        // 💡 SOLUÇÃO: Nome da tabela centralizado e Check Constraint para blindar o Total no banco
        builder.ToTable("sales_order", t =>
        {
            t.HasCheckConstraint("chk_sales_order_total_positive", "total >= 0");
        });

        // Chave primária BIGINT auto-incremental
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .ValueGeneratedOnAdd();

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        // Configuração de precisão decimal para o total do pedido
        builder.Property(o => o.Total)
            .HasPrecision(18, 2)
            .IsRequired();

        // Configuração do campo privado do Domínio Rico (_items de backup)
        var navigation = builder.Metadata.FindNavigation(nameof(SalesOrder.Items));
        navigation?.SetPropertyAccessMode(PropertyAccessMode.Field);

        // =========================================================================
        // 🚀 RELACIONAMENTOS E CHAVES ESTRANGEIRAS (FKs)
        // =========================================================================

        // Relacionamento Um-para-Muitos com os Itens do Pedido (Cascade Delete)
        builder.HasMany(o => o.Items)
            .WithOne(i => i.SalesOrder)
            .HasForeignKey(i => i.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // 💡 NOVO: Mapeamento do relacionamento com o Smart Enum 'Currency'
        builder.HasOne(o => o.Currency)
            .WithMany()
            .HasForeignKey(o => o.CurrencyCode) // Aponta para a propriedade string física da classe
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // Mapeamento do relacionamento com o Smart Enum 'SalesOrderStatus'
        builder.HasOne(o => o.SalesOrderStatus)
            .WithMany()
            .HasForeignKey(o => o.SalesOrderStatusId) // Aponta para a propriedade INT física da classe
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}