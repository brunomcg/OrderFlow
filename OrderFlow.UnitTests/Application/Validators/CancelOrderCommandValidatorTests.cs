using FluentAssertions;
using FluentValidation.TestHelper;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Application.Orders.Validators;
using Xunit;

namespace OrderFlow.UnitTests.Application.Validators
{
    public class CancelOrderCommandValidatorTests
    {
        private readonly CancelOrderCommandValidator _validator = new();

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void Validate_DevePassar_QuandoIdMaiorQueZero(long validId)
        {
            // Arrange
            var command = new CancelOrderCommand(validId);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Validate_DeveRetornarErro_QuandoIdMenorOuIgualZero(long invalidId)
        {
            // Arrange
            var command = new CancelOrderCommand(invalidId);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("O identificador do pedido de venda deve ser maior que zero.");
        }
    }
}
