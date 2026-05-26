using FluentAssertions;
using FluentValidation.TestHelper;
using OrderFlow.Application.Orders.Queries;
using OrderFlow.Application.Orders.Validators;
using Xunit;

namespace OrderFlow.UnitTests.Application.Validators
{
    public class GetOrderByIdQueryValidatorTests
    {
        private readonly GetOrderByIdQueryValidator _validator = new();

        [Theory]
        [InlineData(1)]
        [InlineData(500)]
        public void Validate_DevePassar_QuandoIdMaiorQueZero(long validId)
        {
            // Arrange
            var query = new GetOrderByIdQuery(validId);

            // Act
            var result = _validator.TestValidate(query);

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
            var query = new GetOrderByIdQuery(invalidId);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("O identificador do pedido deve ser maior que zero.");
        }

        [Fact]
        public void Validate_DeveRetornarErro_QuandoIdVazio_Padrao()
        {
            // Arrange
            var query = new GetOrderByIdQuery(default);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("O identificador do pedido é obrigatório.");
        }
    }
}
