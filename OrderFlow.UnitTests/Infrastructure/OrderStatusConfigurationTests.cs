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
            // Arrange: ModelBuilder padrão já traz as convenções necessárias
            var modelBuilder = new ModelBuilder();
            modelBuilder.ApplyConfiguration(new OrderStatusConfiguration());

            // Act: Finaliza o modelo para consolidar os mapeamentos
            var model = modelBuilder.FinalizeModel();
            var entity = model.FindEntityType(typeof(SalesOrderStatus));

            // Assert: A entidade deve existir
            entity.Should().NotBeNull();

            // 1. Tabela
            entity!.GetTableName().Should().Be("sales_order_status");

            // 2. Id: Chave primária e ValueGenerated.Never
            var propId = entity.FindProperty(nameof(SalesOrderStatus.Id));
            propId.Should().NotBeNull();
            var pk = entity.FindPrimaryKey();
            pk!.Properties.Should().Contain(propId);
            propId!.ValueGenerated.Should().Be(ValueGenerated.Never);

            // 3. Name: MaxLength 50, Required
            var propName = entity.FindProperty(nameof(SalesOrderStatus.Name));
            propName.Should().NotBeNull();
            propName!.GetMaxLength().Should().Be(50);
            propName.IsNullable.Should().BeFalse();

            // 4. Description: MaxLength 250, Required
            var propDesc = entity.FindProperty(nameof(SalesOrderStatus.Description));
            propDesc.Should().NotBeNull();
            propDesc!.GetMaxLength().Should().Be(250);
            propDesc.IsNullable.Should().BeFalse();

            // 5. Data seeding: Certifica-se que os dados iniciais estão mapeados
            var seed = entity.GetSeedData();
            seed.Should().NotBeEmpty("A tabela de status deve conter dados iniciais (seeding)");
        }
    }
}