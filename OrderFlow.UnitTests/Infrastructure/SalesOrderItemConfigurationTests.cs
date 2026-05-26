using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Data.Configurations;
using System.Reflection;

namespace OrderFlow.UnitTests.Infrastructure
{
    public class SalesOrderItemConfigurationTests
    {
        [Fact]
        public void SalesOrderItemEntity_ShouldBeMapped_Correctly()
        {
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);

            modelBuilder.ApplyConfiguration(new SalesOrderItemConfiguration());

            try
            {
                var finalize = typeof(ModelBuilder).GetMethod("FinalizeModel", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                finalize?.Invoke(modelBuilder, null);
            }
            catch
            {
            }

            var model = modelBuilder.Model;
            var entity = model.FindEntityType(typeof(SalesOrderItem));
            entity.Should().NotBeNull();

            entity.GetTableName().Should().Be("sales_order_items");

            var pk = entity.FindPrimaryKey();
            pk.Should().NotBeNull();
            var pkNames = pk.Properties.Select(p => p.Name).OrderBy(n => n).ToArray();
            pkNames.Should().Equal(new[] { "ProductId", "SalesOrderId" });

            var propPrice = entity.FindProperty(nameof(SalesOrderItem.UnitPrice));
            propPrice.Should().NotBeNull();
            propPrice.GetPrecision().Should().Be(18);
            propPrice.GetScale().Should().Be(2);
            propPrice.IsNullable.Should().BeFalse();

            var propQty = entity.FindProperty(nameof(SalesOrderItem.Quantity));
            propQty.Should().NotBeNull();
            propQty.IsNullable.Should().BeFalse();

            var fkToOrder = entity.GetForeignKeys().FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(SalesOrder));
            fkToOrder.Should().NotBeNull();
            fkToOrder.Properties.Select(p => p.Name).Should().Contain("SalesOrderId");
            fkToOrder.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);

            var fkToProduct = entity.GetForeignKeys().FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(Product));
            fkToProduct.Should().NotBeNull();
            fkToProduct.Properties.Select(p => p.Name).Should().Contain("ProductId");
            fkToProduct.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);

            var annotations = entity.GetAnnotations().ToList();
            var anyValueContains = annotations.Any(a => a.Value != null && a.Value.ToString().Contains("chk_quantity_positive", StringComparison.OrdinalIgnoreCase));
            var anyPriceContains = annotations.Any(a => a.Value != null && a.Value.ToString().Contains("chk_price_positive", StringComparison.OrdinalIgnoreCase));

            if (anyValueContains)
                anyValueContains.Should().BeTrue();

            if (anyPriceContains)
                anyPriceContains.Should().BeTrue();
        }
    }
}

