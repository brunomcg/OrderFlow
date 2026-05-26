using FluentAssertions;
using FluentValidation.TestHelper;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Application.Orders.Validators;

namespace OrderFlow.UnitTests.Application.Validators
{
    public class CreateOrderCommandValidatorTests
    {
        private readonly CreateOrderCommandValidator _validator = new();

        [Fact]
        public void Validate_DevePassar_QuandoComandoValido()
        {
            // Arrange
            var command = new CreateOrderCommand(
                CustomerId: 1,
                Currency: " usd ",
                Items: new System.Collections.Generic.List<OrderItemInput>
                {
                    new OrderItemInput(1, 1)
                }
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Validate_DeveFalhar_QuandoCustomerIdInvalido(long invalidCustomerId)
        {
            // Arrange
            var command = new CreateOrderCommand(
                CustomerId: invalidCustomerId,
                Currency: "BRL",
                Items: new System.Collections.Generic.List<OrderItemInput> { new OrderItemInput(1, 1) }
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CustomerId);

            // Mensagem apropriada deve mencionar o identificador do cliente
            var errors = result.Errors.Where(e => e.PropertyName == "CustomerId").Select(e => e.ErrorMessage);
            errors.Should().Contain(msg => msg.Contains("identificador do cliente"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_DeveFalhar_QuandoCurrencyNuloOuVazio(string? currency)
        {
            // Arrange
            var command = new CreateOrderCommand(
                CustomerId: 1,
                Currency: currency!,
                Items: new System.Collections.Generic.List<OrderItemInput> { new OrderItemInput(1, 1) }
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Currency)
                  .WithErrorMessage("A moeda (Currency) é obrigatória.");
        }

        [Theory]
        [InlineData("EURR")]
        [InlineData("JPY")]
        public void Validate_DeveFalhar_QuandoCurrencyNaoSuportada_IncluiMensagemDetalhada(string invalidCurrency)
        {
            // Arrange
            var command = new CreateOrderCommand(
                CustomerId: 1,
                Currency: invalidCurrency,
                Items: new System.Collections.Generic.List<OrderItemInput> { new OrderItemInput(1, 1) }
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert: a mensagem deve conter o código rejeitado e a lista de moedas permitidas
            var currencyErrors = result.Errors.Where(e => e.PropertyName == "Currency").ToList();
            currencyErrors.Should().NotBeEmpty();
            var message = currencyErrors.First().ErrorMessage;
            message.Should().Contain(invalidCurrency);
            // Lista de moedas permitidas: BRL, USD, EUR
            message.Should().Contain("BRL");
            message.Should().Contain("USD");
            message.Should().Contain("EUR");
        }

        [Fact]
        public void Validate_DeveFalhar_QuandoItemsNulo()
        {
            // Arrange
            var command = new CreateOrderCommand(
                CustomerId: 1,
                Currency: "BRL",
                Items: null!
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Items)
                  .WithErrorMessage("O pedido deve conter pelo menos um item.");
        }

        [Fact]
        public void Validate_DeveFalhar_QuandoItemsVazio()
        {
            // Arrange
            var command = new CreateOrderCommand(
                CustomerId: 1,
                Currency: "BRL",
                Items: new System.Collections.Generic.List<OrderItemInput>()
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Items)
                  .WithErrorMessage("O pedido deve conter pelo menos um item.");
        }

        [Fact]
        public void Validate_DeveFalhar_QuandoItemPossuiProductIdInvalido()
        {
            // Arrange
            var command = new CreateOrderCommand(
                CustomerId: 1,
                Currency: "BRL",
                Items: new System.Collections.Generic.List<OrderItemInput> { new OrderItemInput(0, 1) }
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert: Ajustado para usar a sintaxe de string do TestHelper mapeando o index correto da lista
            result.ShouldHaveValidationErrorFor("Items[0].ProductId")
                  .WithErrorMessage("O identificador do produto deve ser maior que zero.");
        }

        [Fact]
        public void Validate_DeveFalhar_QuandoItemPossuiQuantityInvalida()
        {
            // Arrange
            var command = new CreateOrderCommand(
                CustomerId: 1,
                Currency: "BRL",
                Items: new System.Collections.Generic.List<OrderItemInput> { new OrderItemInput(1, 0) }
            );

            // Act
            var result = _validator.TestValidate(command);

            // Assert: Ajustado para usar a sintaxe de string do TestHelper mapeando o index correto da lista
            result.ShouldHaveValidationErrorFor("Items[0].Quantity")
                  .WithErrorMessage("A quantidade do item deve ser maior que zero.");
        }
    }
}