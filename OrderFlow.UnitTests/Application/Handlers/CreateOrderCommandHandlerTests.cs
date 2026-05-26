using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Application.Orders.Handlers;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Data;

namespace OrderFlow.UnitTests.Application.Handlers
{
    public class CreateOrderCommandHandlerTests
    {
        private static async Task SeedDatabaseAsync(ApplicationDbContext context)
        {
            context.AddRange(SalesOrderStatus.List());
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task Handle_DeveCriarPedidoComSucesso_EDecrementarEstoques()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            long prod1Id, prod2Id;

            using (var arrangeContext = new ApplicationDbContext(options))
            {
                await SeedDatabaseAsync(arrangeContext);

                var p1 = Product.Create("Produto 1", 5m, 10).Value;
                var p2 = Product.Create("Produto 2", 10m, 20).Value;

                arrangeContext.Products.AddRange(p1, p2);
                await arrangeContext.SaveChangesAsync();

                prod1Id = p1.Id;
                prod2Id = p2.Id;
            }

            long createdOrderId;

            // Act
            using (var actContext = new ApplicationDbContext(options))
            {
                var handler = new CreateOrderCommandHandler(actContext);

                var command = new CreateOrderCommand(
                    CustomerId: 1,
                    Currency: "BRL",
                    Items: new System.Collections.Generic.List<OrderItemInput>
                    {
                        new OrderItemInput(prod1Id, 2),
                        new OrderItemInput(prod2Id, 5)
                    }
                );

                var result = await handler.Handle(command, CancellationToken.None);

                // Assert result
                result.IsSuccess.Should().BeTrue();
                result.Value.Should().BeGreaterThan(0);
                createdOrderId = result.Value;
            }

            // Assert: validate persisted order and product stocks in a fresh context
            using (var assertContext = new ApplicationDbContext(options))
            {
                var order = await assertContext.SalesOrders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == createdOrderId);

                order.Should().NotBeNull();
                order.Items.Should().HaveCount(2);

                var item1 = order.Items.First(i => i.ProductId == prod1Id);
                item1.UnitPrice.Should().Be(5m);
                item1.Quantity.Should().Be(2);

                var item2 = order.Items.First(i => i.ProductId == prod2Id);
                item2.UnitPrice.Should().Be(10m);
                item2.Quantity.Should().Be(5);

                var savedP1 = await assertContext.Products.FindAsync(prod1Id);
                var savedP2 = await assertContext.Products.FindAsync(prod2Id);

                savedP1.AvailableQuantity.Should().Be(8); // 10 - 2
                savedP2.AvailableQuantity.Should().Be(15); // 20 - 5
            }
        }

        [Theory]
        [InlineData(null)]
        public async Task Handle_DeveFalhar_QuandoItemsNuloOuVazio(System.Collections.Generic.List<OrderItemInput> items)
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            using var arrangeContext = new ApplicationDbContext(options);
            await SeedDatabaseAsync(arrangeContext);

            var handler = new CreateOrderCommandHandler(arrangeContext);

            var command = new CreateOrderCommand(1, "BRL", items);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("Items");
            result.Error.Should().Contain("Não é possível criar um pedido sem itens");
        }

        [Fact]
        public async Task Handle_DeveFalhar_QuandoProdutoNaoEncontrado()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            long existingId;

            using (var arrangeContext = new ApplicationDbContext(options))
            {
                await SeedDatabaseAsync(arrangeContext);

                var p1 = Product.Create("Produto 1", 5m, 10).Value;
                arrangeContext.Products.Add(p1);
                await arrangeContext.SaveChangesAsync();
                existingId = p1.Id;
            }

            using (var actContext = new ApplicationDbContext(options))
            {
                var handler = new CreateOrderCommandHandler(actContext);
                var command = new CreateOrderCommand(
                    CustomerId: 1,
                    Currency: "BRL",
                    Items: new System.Collections.Generic.List<OrderItemInput>
                    {
                        new OrderItemInput(999, 1) // non-existent
                    }
                );

                // Act
                var result = await handler.Handle(command, CancellationToken.None);

                // Assert
                result.IsSuccess.Should().BeFalse();
                result.Field.Should().Be("ProductId");
                result.Error.Should().Contain("não foi encontrado");
            }
        }

        [Fact]
        public async Task Handle_DeveFalhar_QuandoEstoqueInsuficiente()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            long prodId;

            using (var arrangeContext = new ApplicationDbContext(options))
            {
                await SeedDatabaseAsync(arrangeContext);

                var p = Product.Create("Produto Low", 3m, 2).Value; // estoque 2
                arrangeContext.Products.Add(p);
                await arrangeContext.SaveChangesAsync();
                prodId = p.Id;
            }

            using (var actContext = new ApplicationDbContext(options))
            {
                var handler = new CreateOrderCommandHandler(actContext);
                var command = new CreateOrderCommand(
                    CustomerId: 1,
                    Currency: "BRL",
                    Items: new System.Collections.Generic.List<OrderItemInput>
                    {
                        new OrderItemInput(prodId, 5) // request 5 > stock 2
                    }
                );

                // Act
                var result = await handler.Handle(command, CancellationToken.None);

                // Assert
                result.IsSuccess.Should().BeFalse();
                result.Error.Should().Contain("Estoque insuficiente");
            }
        }
    }
}
