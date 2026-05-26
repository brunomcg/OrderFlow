using FluentValidation.TestHelper;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Application.Orders.Validators;

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
            var command = new CancelOrderCommand(validId);

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Validate_DeveRetornarErro_QuandoIdMenorOuIgualZero(long invalidId)
        {
            var command = new CancelOrderCommand(invalidId);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("O identificador do pedido de venda deve ser maior que zero.");
        }
    }
}

