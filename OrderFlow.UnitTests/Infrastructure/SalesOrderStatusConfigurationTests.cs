using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Data.Configurations;

namespace OrderFlow.UnitTests.Infrastructure
{
    public class SalesOrderStatusConfigurationTests
    {
        [Fact]
        public void SalesOrderStatusEntity_ShouldBeMapped_Correctly()
        {
            // Arrange
            var modelBuilder = new ModelBuilder();
            modelBuilder.ApplyConfiguration(new SalesOrderStatusConfiguration());

            // Act
            var model = modelBuilder.FinalizeModel();
            var entity = model.FindEntityType(typeof(SalesOrderStatus));

            // Assert
            entity.Should().NotBeNull();

            // 1. Tabela
            entity!.GetTableName().Should().Be("sales_order_status");

            // 2. Id
            var propId = entity.FindProperty(nameof(SalesOrderStatus.Id));
            propId.Should().NotBeNull();
            var pk = entity.FindPrimaryKey();
            pk!.Properties.Should().Contain(propId);
            propId!.ValueGenerated.Should().Be(ValueGenerated.Never);

            // 3. Name
            var propName = entity.FindProperty(nameof(SalesOrderStatus.Name));
            propName.Should().NotBeNull();
            propName!.GetMaxLength().Should().Be(50);
            propName.IsNullable.Should().BeFalse();

            // 4. Description
            var propDesc = entity.FindProperty(nameof(SalesOrderStatus.Description));
            propDesc.Should().NotBeNull();
            propDesc!.GetMaxLength().Should().Be(250);
            propDesc.IsNullable.Should().BeFalse();

            // 5. Seeding
            var seed = entity.GetSeedData();
            seed.Should().NotBeEmpty("A tabela de status deve conter dados iniciais (seeding)");
        }
    }
}