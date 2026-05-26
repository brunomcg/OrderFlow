using FluentAssertions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.UnitTests.Domain
{
    public class SalesOrderItemTests
    {
        [Fact]
        public void Create_DeveCriarItemComSucesso_QuandoParametrosValidos()
        {
            var productId = 5L;
            var unitPrice = 12.34m;
            var quantity = 2;

            var result = SalesOrderItem.Create(productId, unitPrice, quantity);

            result.IsSuccess.Should().BeTrue();
            var item = result.Value;
            item.Should().NotBeNull();
            item.ProductId.Should().Be(productId);
            item.UnitPrice.Should().Be(unitPrice);
            item.Quantity.Should().Be(quantity);
        }

        [Fact]
        public void Create_DevePermitirUnitPriceZero_EInicializarPropriedadesCorretamente()
        {
            var productId = 7L;
            var unitPrice = 0m; // caso de brinde/promocao
            var quantity = 1;

            var result = SalesOrderItem.Create(productId, unitPrice, quantity);

            result.IsSuccess.Should().BeTrue();
            var item = result.Value;
            item.ProductId.Should().Be(productId);
            item.UnitPrice.Should().Be(unitPrice);
            item.Quantity.Should().Be(quantity);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Create_DeveRetornarFalha_QuandoProductIdInvalido(long invalidProductId)
        {
            var unitPrice = 10m;
            var quantity = 1;

            var result = SalesOrderItem.Create(invalidProductId, unitPrice, quantity);

            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("productId");
            result.Error.Should().NotBeNullOrWhiteSpace();
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(-100.5)]
        public void Create_DeveRetornarFalha_QuandoUnitPriceNegativo(decimal negativeUnitPrice)
        {
            var productId = 1L;
            var quantity = 1;

            var result = SalesOrderItem.Create(productId, (decimal)negativeUnitPrice, quantity);

            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("unitPrice");
            result.Error.Should().NotBeNullOrWhiteSpace();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Create_DeveRetornarFalha_QuandoQuantityInvalido(int invalidQuantity)
        {
            var productId = 1L;
            var unitPrice = 5m;

            var result = SalesOrderItem.Create(productId, unitPrice, invalidQuantity);

            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("quantity");
            result.Error.Should().NotBeNullOrWhiteSpace();
        }
    }
}

