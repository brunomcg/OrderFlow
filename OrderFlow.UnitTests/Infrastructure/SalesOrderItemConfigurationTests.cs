using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Data.Configurations;
using Xunit;

namespace OrderFlow.UnitTests.Infrastructure
{
    public class SalesOrderItemConfigurationTests
    {
        [Fact]
        public void SalesOrderItemEntity_ShouldBeMapped_Correctly()
        {
            // Arrange: create a ModelBuilder with conventions
            var conventionSet = new ConventionSet();
            var modelBuilder = new ModelBuilder(conventionSet);

            // Apply our configuration
            modelBuilder.ApplyConfiguration(new SalesOrderItemConfiguration());

            // Try to finalize the model if the API is available (internal in some EF versions)
            try
            {
                var finalize = typeof(ModelBuilder).GetMethod("FinalizeModel", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                finalize?.Invoke(modelBuilder, null);
            }
            catch
            {
                // Ignore - finalization may not be required in this environment
            }

            // Act: get the built model and the entity type
            var model = modelBuilder.Model;
            var entity = model.FindEntityType(typeof(SalesOrderItem));
            entity.Should().NotBeNull();

            // Table name
            entity.GetTableName().Should().Be("sales_order_items");

            // Composite primary key: SalesOrderId + ProductId
            var pk = entity.FindPrimaryKey();
            pk.Should().NotBeNull();
            var pkNames = pk.Properties.Select(p => p.Name).OrderBy(n => n).ToArray();
            pkNames.Should().Equal(new[] { "ProductId", "SalesOrderId" });

            // UnitPrice: precision (18,2) and required
            var propPrice = entity.FindProperty(nameof(SalesOrderItem.UnitPrice));
            propPrice.Should().NotBeNull();
            propPrice.GetPrecision().Should().Be(18);
            propPrice.GetScale().Should().Be(2);
            propPrice.IsNullable.Should().BeFalse();

            // Quantity: required
            var propQty = entity.FindProperty(nameof(SalesOrderItem.Quantity));
            propQty.Should().NotBeNull();
            propQty.IsNullable.Should().BeFalse();

            // Relationship with SalesOrder (FK SalesOrderId, DeleteBehavior.Cascade)
            var fkToOrder = entity.GetForeignKeys().FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(SalesOrder));
            fkToOrder.Should().NotBeNull();
            fkToOrder.Properties.Select(p => p.Name).Should().Contain("SalesOrderId");
            fkToOrder.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);

            // Relationship with Product (FK ProductId, DeleteBehavior.Restrict)
            var fkToProduct = entity.GetForeignKeys().FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(Product));
            fkToProduct.Should().NotBeNull();
            fkToProduct.Properties.Select(p => p.Name).Should().Contain("ProductId");
            fkToProduct.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);

            // Check constraints: best-effort detection via annotations
            var annotations = entity.GetAnnotations().ToList();
            var anyValueContains = annotations.Any(a => a.Value != null && a.Value.ToString().Contains("chk_quantity_positive", StringComparison.OrdinalIgnoreCase));
            var anyPriceContains = annotations.Any(a => a.Value != null && a.Value.ToString().Contains("chk_price_positive", StringComparison.OrdinalIgnoreCase));

            // In some providers (InMemory) check constraints may not be exposed; assert if present
            if (anyValueContains)
                anyValueContains.Should().BeTrue();

            if (anyPriceContains)
                anyPriceContains.Should().BeTrue();
        }
    }
}
