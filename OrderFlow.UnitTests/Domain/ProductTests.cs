using FluentAssertions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.UnitTests.Domain
{
    public class ProductTests
    {
        // Create

        [Fact]
        public void Create_DeveCriarProdutoETrimarONome_QuandoParametrosValidos()
        {
            // Arrange
            var name = "  Produto Test  ";
            var unitPrice = 10.5m;
            var availableQuantity = 5;

            // Act
            var result = Product.Create(name, unitPrice, availableQuantity);

            // Assert
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
            // Arrange
            var unitPrice = 1m;
            var availableQuantity = 1;

            // Act
            var result = Product.Create(name!, unitPrice, availableQuantity);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("name");
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(-100)]
        public void Create_DeveRetornarFalha_QuandoUnitPriceNegativo(decimal unitPrice)
        {
            // Arrange
            var name = "Produto";
            var availableQuantity = 1;

            // Act
            var result = Product.Create(name, (decimal)unitPrice, availableQuantity);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("unitPrice");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Create_DeveRetornarFalha_QuandoAvailableQuantityNegativa(int availableQuantity)
        {
            // Arrange
            var name = "Produto";
            var unitPrice = 1m;

            // Act
            var result = Product.Create(name, unitPrice, availableQuantity);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("availableQuantity");
        }

        // DeductStock

        [Fact]
        public void DeductStock_DeveDeduzirQuantidadeComSucesso()
        {
            // Arrange
            var product = Product.Create("P", 5m, 10).Value;
            var deduct = 4;

            // Act
            var result = product.DeductStock(deduct);

            // Assert
            result.IsSuccess.Should().BeTrue();
            product.AvailableQuantity.Should().Be(6);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void DeductStock_DeveRetornarFalha_QuandoQuantidadeInvalida(int invalid)
        {
            // Arrange
            var product = Product.Create("P", 5m, 10).Value;

            // Act
            var result = product.DeductStock(invalid);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("quantity");
        }

        [Fact]
        public void DeductStock_DeveRetornarFalha_QuandoEstoqueInsuficiente()
        {
            // Arrange
            var product = Product.Create("P", 5m, 2).Value;

            // Act
            var result = product.DeductStock(5);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("AvailableQuantity");
        }

        // ReleaseStock

        [Fact]
        public void ReleaseStock_DeveSomarQuantidadeComSucesso()
        {
            // Arrange
            var product = Product.Create("P", 5m, 2).Value;

            // Act
            var result = product.ReleaseStock(3);

            // Assert
            result.IsSuccess.Should().BeTrue();
            product.AvailableQuantity.Should().Be(5);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public void ReleaseStock_DeveRetornarFalha_QuandoQuantidadeInvalida(int invalid)
        {
            // Arrange
            var product = Product.Create("P", 5m, 2).Value;

            // Act
            var result = product.ReleaseStock(invalid);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("quantity");
        }
    }
}
