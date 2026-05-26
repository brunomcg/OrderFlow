using FluentAssertions;
using OrderFlow.Domain.Entities;

namespace OrderFlow.UnitTests.Domain
{
    public class SalesOrderStatusTests
    {
        [Fact]
        public void List_DeveRetornarTresStatusComPropriedadesPreenchidas()
        {

            var list = SalesOrderStatus.List().ToList();

            list.Should().HaveCount(3);
            list.Select(s => s.Id).Should().Equal(new[] { SalesOrderStatus.Placed.Id, SalesOrderStatus.Confirmed.Id, SalesOrderStatus.Canceled.Id });

            var placed = list[0];
            placed.Id.Should().Be(1);
            placed.Name.Should().Be("Placed");
            placed.Description.Should().Be("Pedido de venda recebido pelo sistema e aguardando processamento.");

            var confirmed = list[1];
            confirmed.Id.Should().Be(2);
            confirmed.Name.Should().Be("Confirmed");
            confirmed.Description.Should().Be("Pagamento aprovado e pedido de venda confirmado.");

            var canceled = list[2];
            canceled.Id.Should().Be(3);
            canceled.Name.Should().Be("Canceled");
            canceled.Description.Should().Be("Pedido de venda cancelado.");
        }

        [Theory]
        [InlineData(1, "Placed")]
        [InlineData(2, "Confirmed")]
        [InlineData(3, "Canceled")]
        public void FromId_DeveRetornarInstanciaCorreta_ParaIdsValidos(int id, string expectedName)
        {

            var status = SalesOrderStatus.FromId(id);

            status.Should().NotBeNull();
            status.Id.Should().Be(id);
            status.Name.Should().Be(expectedName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(4)]
        [InlineData(-1)]
        public void FromId_DeveLancarArgumentException_QuandoIdInvalido(int invalidId)
        {

            Action act = () => SalesOrderStatus.FromId(invalidId);

            act.Should().Throw<ArgumentException>().WithMessage("*ID de status inválido*");
        }

        [Fact]
        public void ToString_DeveRetornarONomeDoStatus()
        {

            var str = SalesOrderStatus.Placed.ToString();

            str.Should().Be(SalesOrderStatus.Placed.Name);
        }
    }
}

