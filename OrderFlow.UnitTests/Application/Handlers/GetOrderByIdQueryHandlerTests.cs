using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Orders.Handlers;
using OrderFlow.Application.Orders.Queries;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Data;

namespace OrderFlow.UnitTests.Application.Handlers
{
    public class GetOrderByIdQueryHandlerTests
    {
        private static async Task SeedDatabaseAsync(ApplicationDbContext context)
        {
            // Insere os Smart Enums de Status na base em memória
            context.AddRange(SalesOrderStatus.List());
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task Handle_DeveRetornarPedidoComSucesso_QuandoPedidoExiste()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            long prodAId, prodBId, orderId;

            // 1) Arrange: criar produtos e pedido em contexto separado
            using (var arrangeContext = new ApplicationDbContext(options))
            {
                await SeedDatabaseAsync(arrangeContext);

                var prodA = Product.Create("Produto A", 10.00m, 100).Value;
                var prodB = Product.Create("Produto B", 20.00m, 100).Value;

                arrangeContext.Products.AddRange(prodA, prodB);
                await arrangeContext.SaveChangesAsync();

                prodAId = prodA.Id;
                prodBId = prodB.Id;

                var order = SalesOrder.Create(1, "BRL").Value;
                var add1 = order.AddItem(prodAId, prodA.UnitPrice, 2);
                add1.IsSuccess.Should().BeTrue();
                var add2 = order.AddItem(prodBId, prodB.UnitPrice, 1);
                add2.IsSuccess.Should().BeTrue();

                arrangeContext.SalesOrders.Add(order);
                await arrangeContext.SaveChangesAsync();

                orderId = order.Id;
            }

            // Act: ler via handler em novo contexto para simular request
            using (var actContext = new ApplicationDbContext(options))
            {
                var handler = new GetOrderByIdQueryHandler(actContext);
                var result = await handler.Handle(new GetOrderByIdQuery(orderId), CancellationToken.None);

                // Assert: resultado de sucesso e DTO preenchido
                result.IsSuccess.Should().BeTrue();
                var dto = result.Value;
                dto.Should().NotBeNull();
                dto.Id.Should().Be(orderId);
                dto.CustomerId.Should().Be(1);
                dto.Status.Should().Be(SalesOrderStatus.Placed.Name);
                dto.Items.Should().HaveCount(2);
                dto.TotalAmount.Should().Be(2 * 10.00m + 1 * 20.00m);

                var itemA = dto.Items.First(i => i.ProductId == prodAId);
                itemA.ProductName.Should().Be("Produto A");
                itemA.UnitPrice.Should().Be(10.00m);
                itemA.Quantity.Should().Be(2);
                itemA.TotalItemPrice.Should().Be(20.00m);

                var itemB = dto.Items.First(i => i.ProductId == prodBId);
                itemB.ProductName.Should().Be("Produto B");
                itemB.UnitPrice.Should().Be(20.00m);
                itemB.Quantity.Should().Be(1);
                itemB.TotalItemPrice.Should().Be(20.00m);
            }
        }

        [Fact]
        public async Task Handle_DeveRetornarFalha_QuandoPedidoNaoEncontrado()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            using (var arrangeContext = new ApplicationDbContext(options))
            {
                await SeedDatabaseAsync(arrangeContext);
                // sem inserir pedidos
            }

            // Act
            using (var actContext = new ApplicationDbContext(options))
            {
                var handler = new GetOrderByIdQueryHandler(actContext);
                var result = await handler.Handle(new GetOrderByIdQuery(999), CancellationToken.None);

                // Assert
                result.IsSuccess.Should().BeFalse();
                result.Error.Should().Contain("não foi encontrado");
            }
        }
    }
}
