using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Infrastructure.Data.Mappings;

public class OrderStatusConfiguration : IEntityTypeConfiguration<SalesOrderStatus>
{
    public void Configure(EntityTypeBuilder<SalesOrderStatus> builder)
    {
        builder.ToTable("sales_order_status");

        // Define a chave primária
        builder.HasKey(os => os.Id);

        // Define que o Id não será auto-incremento (Identity), pois nós controlamos os IDs fixos no C#
        builder.Property(os => os.Id)
            .ValueGeneratedNever();

        builder.Property(os => os.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(os => os.Description)
            .HasMaxLength(250)
            .IsRequired();

        // ==========================================
        // DATA SEEDING: Alimenta a tabela de domínio automaticamente
        // ==========================================
        builder.HasData(SalesOrderStatus.List());
    }
}