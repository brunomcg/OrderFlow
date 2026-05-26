using FluentAssertions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.UnitTests.Domain
{
    public class ProductTests
    {

        [Fact]
        public void Create_DeveCriarProdutoETrimarONome_QuandoParametrosValidos()
        {
            var name = "  Produto Test  ";
            var unitPrice = 10.5m;
            var availableQuantity = 5;

            var result = Product.Create(name, unitPrice, availableQuantity);

            result.IsSuccess.Should().BeTrue();
            var product = result.Value;
            product.Name.Should().Be("Produto Test");
            product.UnitPrice.Should().Be(unitPrice);
            product.AvailableQuantity.Should().Be(availableQuantity);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_DeveRetornarFalha_QuandoNomeInvalido(string name)
        {
            var unitPrice = 1m;
            var availableQuantity = 1;

            var result = Product.Create(name!, unitPrice, availableQuantity);

            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("name");
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(-100)]
        public void Create_DeveRetornarFalha_QuandoUnitPriceNegativo(decimal unitPrice)
        {
            var name = "Produto";
            var availableQuantity = 1;

            var result = Product.Create(name, (decimal)unitPrice, availableQuantity);

            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("unitPrice");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Create_DeveRetornarFalha_QuandoAvailableQuantityNegativa(int availableQuantity)
        {
            var name = "Produto";
            var unitPrice = 1m;

            var result = Product.Create(name, unitPrice, availableQuantity);

            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("availableQuantity");
        }


        [Fact]
        public void DeductStock_DeveDeduzirQuantidadeComSucesso()
        {
            var product = Product.Create("P", 5m, 10).Value;
            var deduct = 4;

            var result = product.DeductStock(deduct);

            result.IsSuccess.Should().BeTrue();
            product.AvailableQuantity.Should().Be(6);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void DeductStock_DeveRetornarFalha_QuandoQuantidadeInvalida(int invalid)
        {
            var product = Product.Create("P", 5m, 10).Value;

            var result = product.DeductStock(invalid);

            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("quantity");
        }

        [Fact]
        public void DeductStock_DeveRetornarFalha_QuandoEstoqueInsuficiente()
        {
            var product = Product.Create("P", 5m, 2).Value;

            var result = product.DeductStock(5);

            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("AvailableQuantity");
        }


        [Fact]
        public void ReleaseStock_DeveSomarQuantidadeComSucesso()
        {
            var product = Product.Create("P", 5m, 2).Value;

            var result = product.ReleaseStock(3);

            result.IsSuccess.Should().BeTrue();
            product.AvailableQuantity.Should().Be(5);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public void ReleaseStock_DeveRetornarFalha_QuandoQuantidadeInvalida(int invalid)
        {
            var product = Product.Create("P", 5m, 2).Value;

            var result = product.ReleaseStock(invalid);

            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("quantity");
        }
    }
}

