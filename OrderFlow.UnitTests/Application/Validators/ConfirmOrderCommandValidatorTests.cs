using FluentAssertions;
using FluentValidation.TestHelper;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Application.Orders.Validators;
using Xunit;

namespace OrderFlow.UnitTests.Application.Validators
{
    public class ConfirmOrderCommandValidatorTests
    {
        private readonly CinfirmOrderCommandValidator _validator = new();

        [Theory]
        [InlineData(1)]
        [InlineData(42)]
        public void Validate_DevePassar_QuandoIdMaiorQueZero(long validId)
        {
            var command = new ConfirmOrderCommand(validId);

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-50)]
        public void Validate_DeveRetornarErro_QuandoIdMenorOuIgualZero(long invalidId)
        {
            var command = new ConfirmOrderCommand(invalidId);

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("O identificador do pedido de venda deve ser maior que zero.");
        }
    }
}

