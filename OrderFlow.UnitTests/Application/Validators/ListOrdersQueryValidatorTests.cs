using FluentAssertions;
using FluentValidation.TestHelper;
using OrderFlow.Application.Orders.Queries;
using OrderFlow.Application.Orders.Validators;
using OrderFlow.Domain.Entities;

namespace OrderFlow.UnitTests.Application.Validators
{
    public class ListOrdersQueryValidatorTests
    {
        private readonly ListOrdersQueryValidator _validator = new();

        [Fact]
        public void Validate_DevePassar_QuandoPadraoDePaginacaoValidoEFiltrosNulos()
        {
            // Arrange
            var query = new ListOrdersQuery(
                Id: null,
                CustomerId: null,
                Status: null,
                From: null,
                To: null,
                Page: 1,
                PageSize: 10
            );

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_DevePassar_QuandoTodosFiltrosPreenchidosCorretamente()
        {
            // Arrange
            var query = new ListOrdersQuery(
                Id: 1,
                CustomerId: 1,
                Status: SalesOrderStatus.Placed.Id,
                From: "2026-01-01",
                To: "2026-01-10",
                Page: 1,
                PageSize: 10
            );

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_DeveFalhar_QuandoPageMenorOuIgualZero(int invalidPage)
        {
            // Arrange
            var query = new ListOrdersQuery(null, null, null, null, null, invalidPage, 10);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Page)
                  .WithErrorMessage("O número da página deve ser maior que zero.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void Validate_DeveFalhar_QuandoPageSizeForaDoIntervalo(int invalidPageSize)
        {
            // Arrange
            var query = new ListOrdersQuery(null, null, null, null, null, 1, invalidPageSize);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                  .WithErrorMessage("O tamanho da página deve ser entre 1 e 100 itens.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Validate_DeveFalhar_QuandoIdInformadoInvalido(long invalidId)
        {
            // Arrange
            var query = new ListOrdersQuery(invalidId, null, null, null, null, 1, 10);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("O identificador do pedido de venda deve ser maior que zero.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public void Validate_DeveFalhar_QuandoCustomerIdInformadoInvalido(long invalidCustomerId)
        {
            // Arrange
            var query = new ListOrdersQuery(null, invalidCustomerId, null, null, null, 1, 10);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CustomerId)
                  .WithErrorMessage("O identificador do cliente deve ser maior que zero.");
        }

        [Fact]
        public void Validate_DeveFalhar_QuandoStatusInformadoNaoExiste()
        {
            // Arrange
            var invalidStatus = 5;
            var query = new ListOrdersQuery(null, null, invalidStatus, null, null, 1, 10);

            // Act
            var result = _validator.TestValidate(query);

            // Assert: valida mensagem dinâmica contendo lista de status permitidos
            var errors = result.Errors.Where(e => e.PropertyName == "Status").ToList();
            errors.Should().NotBeEmpty();
            var message = errors.First().ErrorMessage;
            message.Should().Contain(invalidStatus.ToString());
            message.Should().Contain("1-Placed");
            message.Should().Contain("2-Confirmed");
            message.Should().Contain("3-Canceled");
        }

        [Theory]
        [InlineData("isto-nao-e-uma-data")]
        [InlineData("2026-13-01")] // Mês 13 inválido
        [InlineData("2026-12-45")] // Dia 45 inválido
        public void Validate_DeveFalhar_QuandoFromFormatoInvalido(string invalidFrom)
        {
            // Arrange
            var query = new ListOrdersQuery(null, null, null, invalidFrom, null, 1, 10);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.From)
                  .WithErrorMessage("A data inicial (From) possui um formato inválido. Utilize o padrão AAAA-MM-DD.");
        }

        [Theory]
        [InlineData("isto-nao-e-uma-data")]
        [InlineData("2026-13-01")] // Mês 13 inválido
        [InlineData("2026-12-45")] // Dia 45 inválido
        public void Validate_DeveFalhar_QuandoToFormatoInvalido(string invalidTo)
        {
            // Arrange
            var query = new ListOrdersQuery(null, null, null, null, invalidTo, 1, 10);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.To)
                  .WithErrorMessage("A data final (To) possui um formato inválido. Utilize o padrão AAAA-MM-DD.");
        }

        [Fact]
        public void Validate_DeveFalhar_QuandoFromMaiorQueTo()
        {
            // Arrange
            var query = new ListOrdersQuery(null, null, null, "2026-05-20", "2026-05-10", 1, 10);

            // Act
            var result = _validator.TestValidate(query);

            // Assert: regra de negócio sobre o intervalo
            result.ShouldHaveValidationErrorFor(x => x.From)
                  .WithErrorMessage("A data inicial (From) não pode ser maior que a data final (To).");
        }
    }
}