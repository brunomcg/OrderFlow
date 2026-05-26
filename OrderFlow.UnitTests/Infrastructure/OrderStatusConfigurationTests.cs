using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Data.Mappings;

namespace OrderFlow.UnitTests.Infrastructure
{
    public class OrderStatusConfigurationTests
    {
        [Fact]
        public void SalesOrderStatusEntity_ShouldBeMapped_Correctly()
        {
            var modelBuilder = new ModelBuilder();
            modelBuilder.ApplyConfiguration(new OrderStatusConfiguration());

            var model = modelBuilder.FinalizeModel();
            var entity = model.FindEntityType(typeof(SalesOrderStatus));

            entity.Should().NotBeNull();

            entity!.GetTableName().Should().Be("sales_order_status");

            var propId = entity.FindProperty(nameof(SalesOrderStatus.Id));
            propId.Should().NotBeNull();
            var pk = entity.FindPrimaryKey();
            pk!.Properties.Should().Contain(propId);
            propId!.ValueGenerated.Should().Be(ValueGenerated.Never);

            var propName = entity.FindProperty(nameof(SalesOrderStatus.Name));
            propName.Should().NotBeNull();
            propName!.GetMaxLength().Should().Be(50);
            propName.IsNullable.Should().BeFalse();

            var propDesc = entity.FindProperty(nameof(SalesOrderStatus.Description));
            propDesc.Should().NotBeNull();
            propDesc!.GetMaxLength().Should().Be(250);
            propDesc.IsNullable.Should().BeFalse();

            var seed = entity.GetSeedData();
            seed.Should().NotBeEmpty("A tabela de status deve conter dados iniciais (seeding)");
        }
    }
}
