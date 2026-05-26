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
            // Arrange: Usamos o ModelBuilder diretamente para validar o mapeamento
            var builder = new ModelBuilder();
            new SalesOrderConfiguration().Configure(builder.Entity<SalesOrder>());
            new SalesOrderItemConfiguration().Configure(builder.Entity<SalesOrderItem>());

            // Finaliza o modelo para inicializar as dependências de metadados
            var model = builder.FinalizeModel();
            var entity = model.FindEntityType(typeof(SalesOrder));

            // Assert: A entidade deve existir
            entity.Should().NotBeNull();

            // 1. Tabela
            entity!.GetTableName().Should().Be("sales_order");

            // 2. Id: chave primária e ValueGeneratedOnAdd
            var propId = entity.FindProperty(nameof(SalesOrder.Id));
            propId.Should().NotBeNull();
            var pk = entity.FindPrimaryKey();
            pk!.Properties.Should().Contain(propId);
            propId!.ValueGenerated.Should().Be(ValueGenerated.OnAdd);

            // 3. Total: precisão (18,2) e requerido
            var propTotal = entity.FindProperty(nameof(SalesOrder.Total));
            propTotal.Should().NotBeNull();
            propTotal!.GetPrecision().Should().Be(18);
            propTotal.GetScale().Should().Be(2);
            propTotal.IsNullable.Should().BeFalse();

            // 4. Configuração de acesso: Items usa PropertyAccessMode.Field ou PreferField
            var navigation = entity.FindNavigation(nameof(SalesOrder.Items));
            navigation.Should().NotBeNull();

            // O EF Core pode converter Field para PreferField internamente
            var accessMode = navigation!.GetPropertyAccessMode();
            accessMode.Should().BeOneOf(PropertyAccessMode.Field, PropertyAccessMode.PreferField);

            // 5. Relacionamento SalesOrder -> SalesOrderItem (1:n)
            var itemEntity = model.FindEntityType(typeof(SalesOrderItem));
            itemEntity.Should().NotBeNull();

            var fk = itemEntity!.GetForeignKeys().FirstOrDefault(f => f.PrincipalEntityType == entity);
            fk.Should().NotBeNull();
            fk!.Properties.Select(p => p.Name).Should().Contain("SalesOrderId");
            fk.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);

            // 6. Check Constraint: Validação no modelo finalizado
            var checkConstraint = entity.GetCheckConstraints()
                .FirstOrDefault(c => c.Name == "chk_sales_order_total_positive");

            checkConstraint.Should().NotBeNull("A check constraint 'chk_sales_order_total_positive' deve estar configurada no modelo");
        }
    }
}