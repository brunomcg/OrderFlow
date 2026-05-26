using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderFlow.Domain.Entities;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("currency");

        builder.HasKey(c => c.Code);

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

        builder.HasData(Currency.List());
    }
}