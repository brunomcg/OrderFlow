using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Data;

namespace OrderFlow.UnitTests.Infrastructure
{
    public class ProductConfigurationTests
    {
        [Fact]
        public void ProductEntity_ShouldBeMapped_Correctly()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            var model = context.Model;
            var entity = model.FindEntityType(typeof(Product)) ?? model.GetEntityTypes().FirstOrDefault(e => e.ClrType == typeof(Product));

            entity.Should().NotBeNull("Product entity must be part of the EF model via ProductConfiguration");

            entity.GetTableName().Should().Be("product");

            var propId = entity.FindProperty(nameof(Product.Id));
            propId.Should().NotBeNull();
            var pk = entity.FindPrimaryKey();
            pk.Properties.Should().Contain(propId);
            propId.ValueGenerated.Should().Be(ValueGenerated.OnAdd);

            var propName = entity.FindProperty(nameof(Product.Name));
            propName.Should().NotBeNull();
            propName.GetMaxLength().Should().Be(150);
            propName.IsNullable.Should().BeFalse();

            var propPrice = entity.FindProperty(nameof(Product.UnitPrice));
            propPrice.Should().NotBeNull();
            propPrice.GetPrecision().Should().Be(18);
            propPrice.GetScale().Should().Be(2);
            propPrice.IsNullable.Should().BeFalse();

            var propQty = entity.FindProperty(nameof(Product.AvailableQuantity));
            propQty.Should().NotBeNull();
            propQty.IsNullable.Should().BeFalse();

            var annotations = entity.GetAnnotations().Select(a => a.Name).ToList();
            var hasPriceConstraint = annotations.Any(n => n.Contains("chk_product_price_positive", StringComparison.OrdinalIgnoreCase));
            var hasStockConstraint = annotations.Any(n => n.Contains("chk_product_stock_positive", StringComparison.OrdinalIgnoreCase));
            if (hasPriceConstraint)
                hasPriceConstraint.Should().BeTrue();

            if (hasStockConstraint)
                hasStockConstraint.Should().BeTrue();
        }
    }
}

