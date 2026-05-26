using FluentAssertions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.UnitTests.Domain
{
    public class SalesOrderTests
    {

        [Fact]
        public void Create_DeveCriarPedidoComSucesso_QuandoParametrosEMoedaValidos()
        {
            var customerId = 1L;
            var currency = "BRL"; // moeda suportada pelo Smart Enum

            var result = SalesOrder.Create(customerId, currency);

            result.IsSuccess.Should().BeTrue();
            var order = result.Value;
            order.Should().NotBeNull();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Placed.Id);
            order.Total.Should().Be(0m);
        }

        [Fact]
        public void Create_DeveFalhar_QuandoCustomerIdMenorOuIgualZero()
        {
            var customerId = 0L; // inválido
            var currency = "BRL";

            var result = SalesOrder.Create(customerId, currency);

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
            var customerId = 1L;

            var result = SalesOrder.Create(customerId, currency!);

            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("currency");
        }

        [Fact]
        public void Create_DeveFalhar_QuandoMoedaNaoSuportada()
        {
            var customerId = 1L;
            var currency = "XYZ"; // não existe no Smart Enum

            var result = SalesOrder.Create(customerId, currency);

            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("currency");
        }


        [Fact]
        public void AddItem_DeveAdicionarItemERecalcularTotal()
        {
            var orderResult = SalesOrder.Create(1, "BRL");
            orderResult.IsSuccess.Should().BeTrue();
            var order = orderResult.Value;

            var productId = 10L;
            var unitPrice = 15.50m;
            var quantity = 3;

            var addResult = order.AddItem(productId, unitPrice, quantity);

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
            var orderResult = SalesOrder.Create(1, "BRL");
            orderResult.IsSuccess.Should().BeTrue();
            var order = orderResult.Value;

            var addResult = order.AddItem(0, 10m, 1);

            addResult.IsSuccess.Should().BeFalse();
            addResult.Field.Should().Be("productId");
            order.Items.Should().BeEmpty();
            order.Total.Should().Be(0m);
        }


        [Fact]
        public void Confirm_DeveAlterarStatusParaConfirmed_SeStatusAtualForPlaced()
        {
            var order = SalesOrder.Create(1, "BRL").Value;
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Placed.Id);

            var result = order.Confirm();

            result.IsSuccess.Should().BeTrue();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Confirmed.Id);
        }

        [Fact]
        public void Confirm_DeveSerIdempotente_SeJaEstaConfirmed()
        {
            var order = SalesOrder.Create(1, "BRL").Value;
            var first = order.Confirm();
            first.IsSuccess.Should().BeTrue();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Confirmed.Id);

            var second = order.Confirm();

            second.IsSuccess.Should().BeTrue();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Confirmed.Id);
        }

        [Fact]
        public void Confirm_DeveRetornarFalha_SePedidoEstiverCanceled()
        {
            var order = SalesOrder.Create(1, "BRL").Value;
            var cancel = order.Cancel();
            cancel.IsSuccess.Should().BeTrue();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Canceled.Id);

            var result = order.Confirm();

            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("Status");
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Canceled.Id);
        }


        [Fact]
        public void Cancel_DeveAlterarStatusParaCanceled_SeStatusForPlaced()
        {
            var order = SalesOrder.Create(1, "BRL").Value;

            var result = order.Cancel();

            result.IsSuccess.Should().BeTrue();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Canceled.Id);
        }

        [Fact]
        public void Cancel_DeveAlterarStatusParaCanceled_SeStatusForConfirmed()
        {
            var order = SalesOrder.Create(1, "BRL").Value;
            order.Confirm();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Confirmed.Id);

            var result = order.Cancel();

            result.IsSuccess.Should().BeTrue();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Canceled.Id);
        }

        [Fact]
        public void Cancel_DeveSerIdempotente_SeJaEstaCanceled()
        {
            var order = SalesOrder.Create(1, "BRL").Value;
            var first = order.Cancel();
            first.IsSuccess.Should().BeTrue();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Canceled.Id);

            var second = order.Cancel();

            second.IsSuccess.Should().BeTrue();
            order.SalesOrderStatusId.Should().Be(SalesOrderStatus.Canceled.Id);
        }
    }
}

