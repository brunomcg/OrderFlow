using FluentAssertions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.UnitTests.Domain
{
    public class SalesOrderTests
    {
        // Criação do Pedido (Create)

        [Fact]
        public void Create_DeveCriarPedidoComSucesso_QuandoParametrosEMoedaValidos()
        {
            // Arrange
            var customerId = 1L;
            var currency = "BRL"; // moeda suportada pelo Smart Enum

            // Act
            var result = SalesOrder.Create(customerId, currency);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var order = result.Value;
            order.Should().NotBeNull();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Placed.Id);
            order.Total.Should().Be(0m);
        }

        [Fact]
        public void Create_DeveFalhar_QuandoCustomerIdMenorOuIgualZero()
        {
            // Arrange
            var customerId = 0L; // inválido
            var currency = "BRL";

            // Act
            var result = SalesOrder.Create(customerId, currency);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("customerId");
            result.Error.Should().NotBeNullOrWhiteSpace();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_DeveFalhar_QuandoMoedaNulaOuVazia(string? currency)
        {
            // Arrange
            var customerId = 1L;

            // Act
            // força passagem de null quando necessário
            var result = SalesOrder.Create(customerId, currency!);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("currency");
        }

        [Fact]
        public void Create_DeveFalhar_QuandoMoedaNaoSuportada()
        {
            // Arrange
            var customerId = 1L;
            var currency = "XYZ"; // não existe no Smart Enum

            // Act
            var result = SalesOrder.Create(customerId, currency);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("currency");
        }

        // Adição de Itens (AddItem)

        [Fact]
        public void AddItem_DeveAdicionarItemERecalcularTotal()
        {
            // Arrange
            var orderResult = SalesOrder.Create(1, "BRL");
            orderResult.IsSuccess.Should().BeTrue();
            var order = orderResult.Value;

            var productId = 10L;
            var unitPrice = 15.50m;
            var quantity = 3;

            // Act
            var addResult = order.AddItem(productId, unitPrice, quantity);

            // Assert
            addResult.IsSuccess.Should().BeTrue();
            order.Items.Should().HaveCount(1);
            var item = order.Items.First();
            item.ProductId.Should().Be(productId);
            item.UnitPrice.Should().Be(unitPrice);
            item.Quantity.Should().Be(quantity);
            order.Total.Should().Be(unitPrice * quantity);
        }

        [Fact]
        public void AddItem_DevePropagarFalha_QuandoItemEhInvalido()
        {
            // Arrange
            var orderResult = SalesOrder.Create(1, "BRL");
            orderResult.IsSuccess.Should().BeTrue();
            var order = orderResult.Value;

            // Act
            // produto inválido (productId <= 0) deve gerar falha na criação do SalesOrderItem
            var addResult = order.AddItem(0, 10m, 1);

            // Assert
            addResult.IsSuccess.Should().BeFalse();
            addResult.Field.Should().Be("productId");
            order.Items.Should().BeEmpty();
            order.Total.Should().Be(0m);
        }

        // Confirmação do Pedido (Confirm)

        [Fact]
        public void Confirm_DeveAlterarStatusParaConfirmed_SeStatusAtualForPlaced()
        {
            // Arrange
            var order = SalesOrder.Create(1, "BRL").Value;
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Placed.Id);

            // Act
            var result = order.Confirm();

            // Assert
            result.IsSuccess.Should().BeTrue();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Confirmed.Id);
        }

        [Fact]
        public void Confirm_DeveSerIdempotente_SeJaEstaConfirmed()
        {
            // Arrange
            var order = SalesOrder.Create(1, "BRL").Value;
            var first = order.Confirm();
            first.IsSuccess.Should().BeTrue();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Confirmed.Id);

            // Act
            var second = order.Confirm();

            // Assert
            second.IsSuccess.Should().BeTrue();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Confirmed.Id);
        }

        [Fact]
        public void Confirm_DeveRetornarFalha_SePedidoEstiverCanceled()
        {
            // Arrange
            var order = SalesOrder.Create(1, "BRL").Value;
            var cancel = order.Cancel();
            cancel.IsSuccess.Should().BeTrue();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Canceled.Id);

            // Act
            var result = order.Confirm();

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("Status");
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Canceled.Id);
        }

        // Cancelamento do Pedido (Cancel)

        [Fact]
        public void Cancel_DeveAlterarStatusParaCanceled_SeStatusForPlaced()
        {
            // Arrange
            var order = SalesOrder.Create(1, "BRL").Value;

            // Act
            var result = order.Cancel();

            // Assert
            result.IsSuccess.Should().BeTrue();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Canceled.Id);
        }

        [Fact]
        public void Cancel_DeveAlterarStatusParaCanceled_SeStatusForConfirmed()
        {
            // Arrange
            var order = SalesOrder.Create(1, "BRL").Value;
            order.Confirm();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Confirmed.Id);

            // Act
            var result = order.Cancel();

            // Assert
            result.IsSuccess.Should().BeTrue();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Canceled.Id);
        }

        [Fact]
        public void Cancel_DeveSerIdempotente_SeJaEstaCanceled()
        {
            // Arrange
            var order = SalesOrder.Create(1, "BRL").Value;
            var first = order.Cancel();
            first.IsSuccess.Should().BeTrue();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Canceled.Id);

            // Act
            var second = order.Cancel();

            // Assert
            second.IsSuccess.Should().BeTrue();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Canceled.Id);
        }
    }
}
