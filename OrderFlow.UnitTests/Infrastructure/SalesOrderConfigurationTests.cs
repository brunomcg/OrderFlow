using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Data.Configurations;

namespace OrderFlow.UnitTests.Infrastructure
{
    public class SalesOrderConfigurationTests
    {
        [Fact]
        public void SalesOrderEntity_ShouldBeMapped_Correctly()
        {
            var builder = new ModelBuilder();
            new SalesOrderConfiguration().Configure(builder.Entity<SalesOrder>());
            new SalesOrderItemConfiguration().Configure(builder.Entity<SalesOrderItem>());

            var model = builder.FinalizeModel();
            var entity = model.FindEntityType(typeof(SalesOrder));

            entity.Should().NotBeNull();

            entity!.GetTableName().Should().Be("sales_order");

            var propId = entity.FindProperty(nameof(SalesOrder.Id));
            propId.Should().NotBeNull();
            var pk = entity.FindPrimaryKey();
            pk!.Properties.Should().Contain(propId);
            propId!.ValueGenerated.Should().Be(ValueGenerated.OnAdd);

            var propTotal = entity.FindProperty(nameof(SalesOrder.Total));
            propTotal.Should().NotBeNull();
            propTotal!.GetPrecision().Should().Be(18);
            propTotal.GetScale().Should().Be(2);
            propTotal.IsNullable.Should().BeFalse();

            var navigation = entity.FindNavigation(nameof(SalesOrder.Items));
            navigation.Should().NotBeNull();

            var accessMode = navigation!.GetPropertyAccessMode();
            accessMode.Should().BeOneOf(PropertyAccessMode.Field, PropertyAccessMode.PreferField);

            var itemEntity = model.FindEntityType(typeof(SalesOrderItem));
            itemEntity.Should().NotBeNull();

            var fk = itemEntity!.GetForeignKeys().FirstOrDefault(f => f.PrincipalEntityType == entity);
            fk.Should().NotBeNull();
            fk!.Properties.Select(p => p.Name).Should().Contain("SalesOrderId");
            fk.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);

            var checkConstraint = entity.GetCheckConstraints()
                .FirstOrDefault(c => c.Name == "chk_sales_order_total_positive");

            checkConstraint.Should().NotBeNull("A check constraint 'chk_sales_order_total_positive' deve estar configurada no modelo");
        }
    }
}
