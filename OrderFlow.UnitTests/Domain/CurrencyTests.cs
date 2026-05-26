using FluentAssertions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.UnitTests.Domain
{
    public class CurrencyTests
    {
        [Fact]
        public void List_DeveRetornarMoedasSuportadasComPropriedadesPreenchidas()
        {
            var list = Currency.List().ToList();

            list.Should().HaveCount(3);
            list.Select(c => c.Code).Should().BeEquivalentTo(new[] { "BRL", "USD", "EUR" }, options => options.WithStrictOrdering());

            var brl = list.First(c => c.Code == "BRL");
            brl.Symbol.Should().Be("R$");
            brl.Name.Should().Be("Real Brasileiro");

            var usd = list.First(c => c.Code == "USD");
            usd.Symbol.Should().Be("$");
            usd.Name.Should().Be("United States Dollar");

            var eur = list.First(c => c.Code == "EUR");
            eur.Symbol.Should().Be("€");
            eur.Name.Should().Be("Euro");
        }

        [Theory]
        [InlineData("BRL")]
        [InlineData("brl")]
        [InlineData(" Brl ")]
        [InlineData("USD")]
        [InlineData(" usd ")]
        [InlineData("EUR")]
        [InlineData(" eur ")]
        public void FromCode_DeveRetornarInstanciaCorreta_QuandoCodigoValido(string input)
        {

            var currency = Currency.FromCode(input);

            currency.Should().NotBeNull();
            currency.Code.Should().Be(currency.ToString());
            Currency.List().Select(c => c.Code).Should().Contain(currency.Code);
        }

        [Theory]
        [InlineData("JPY")]
        [InlineData("EURR")]
        [InlineData("ABC")]
        public void FromCode_DeveLancarArgumentException_QuandoCodigoInvalido(string invalid)
        {
            Action act = () => Currency.FromCode(invalid);

            act.Should().Throw<ArgumentException>().WithMessage("*Moeda não suportada*");
        }

        [Fact]
        public void ToString_DeveRetornarOCodigoDaMoeda()
        {
            var str = Currency.BRL.ToString();

            str.Should().Be("BRL");
        }
    }
}

