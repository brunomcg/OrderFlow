using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        // Nome da tabela em minúsculo (Padrão Postgres)
        builder.ToTable("currency");

        // Define a chave primária como o código ISO (string de 3 caracteres)
        builder.HasKey(c => c.Code);

        // Desativa o auto-incremento, pois controlamos os códigos rigidamente no C#
        builder.Property(c => c.Code)
            .HasMaxLength(3)
            .IsFixedLength() // CHAR(3)
            .IsRequired();

        builder.Property(c => c.Symbol)
            .HasMaxLength(5)
            .IsRequired();

        builder.Property(c => c.Name)
            .HasMaxLength(50)
            .IsRequired();

        // ==========================================
        // DATA SEEDING: Alimenta a tabela de moedas automaticamente
        // ==========================================
        builder.HasData(Currency.List());
    }
}