using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Application.Orders.Handlers;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Data;
using Xunit;

namespace OrderFlow.UnitTests.Application.Handlers
{
    public class CancelOrderCommandHandlerTests
    {
        private async Task SeedDatabaseAsync(DbContextOptions<ApplicationDbContext> options)
        {
            using var context = new ApplicationDbContext(options);

            if (!await context.Set<SalesOrderStatus>().AnyAsync())
            {
                context.Set<SalesOrderStatus>().AddRange(SalesOrderStatus.List());
                await context.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task Handle_DeveCancelarPedidoEDevolverEstoque_ComSucesso()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            await SeedDatabaseAsync(options);

            long orderId;

            using (var arrangeContext = new ApplicationDbContext(options))
            {
                var product = Product.Create("Produto A", 5m, 10).Value;
                arrangeContext.Products.Add(product);
                await arrangeContext.SaveChangesAsync();

                var order = SalesOrder.Create(1, "BRL").Value;
                var addItemResult = order.AddItem(product.Id, product.UnitPrice, 3);
                addItemResult.IsSuccess.Should().BeTrue();

                arrangeContext.SalesOrders.Add(order);
                await arrangeContext.SaveChangesAsync();

                orderId = order.Id;
            }

            using (var actContext = new ApplicationDbContext(options))
            {
                var handler = new CancelOrderCommandHandler(actContext);
                var result = await handler.Handle(new CancelOrderCommand(orderId), CancellationToken.None);

                result.IsSuccess.Should().BeTrue();
            }

            using (var assertContext = new ApplicationDbContext(options))
            {
                var savedOrder = await assertContext.SalesOrders
                    .Include(o => o.SalesOrderStatus)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                savedOrder.Should().NotBeNull();
                savedOrder!.SalesOrderStatusId.Should().Be(SalesOrderStatus.Canceled.Id);

                var savedProduct = await assertContext.Products.FirstAsync();
                savedProduct.AvailableQuantity.Should().Be(13);
            }
        }

        [Fact]
        public async Task Handle_DeveSerIdempotente_QuandoPedidoJaEstaCancelado()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            await SeedDatabaseAsync(options);

            long orderId;

            using (var arrangeContext = new ApplicationDbContext(options))
            {
                var product = Product.Create("Produto B", 2m, 10).Value;
                arrangeContext.Products.Add(product);
                await arrangeContext.SaveChangesAsync();

                var order = SalesOrder.Create(1, "BRL").Value;
                var addItemResult = order.AddItem(product.Id, product.UnitPrice, 3);
                addItemResult.IsSuccess.Should().BeTrue();

                var cancelResult = order.Cancel();
                cancelResult.IsSuccess.Should().BeTrue();

                arrangeContext.SalesOrders.Add(order);
                await arrangeContext.SaveChangesAsync();

                orderId = order.Id;
            }

            using (var actContext = new ApplicationDbContext(options))
            {
                var handler = new CancelOrderCommandHandler(actContext);
                var result = await handler.Handle(new CancelOrderCommand(orderId), CancellationToken.None);

                result.IsSuccess.Should().BeTrue();
            }

            using (var assertContext = new ApplicationDbContext(options))
            {
                var savedOrder = await assertContext.SalesOrders
                    .Include(o => o.SalesOrderStatus)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                savedOrder.Should().NotBeNull();
                savedOrder!.SalesOrderStatusId.Should().Be(SalesOrderStatus.Canceled.Id);

                var savedProduct = await assertContext.Products.FirstAsync();
                savedProduct.AvailableQuantity.Should().Be(10);
            }
        }

        [Fact]
        public async Task Handle_DeveRetornarFalha_QuandoPedidoNaoEncontrado()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            await SeedDatabaseAsync(options);

            using var context = new ApplicationDbContext(options);
            var handler = new CancelOrderCommandHandler(context);

            var result = await handler.Handle(new CancelOrderCommand(999), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("Id");
            result.Error.Should().Contain("não foi encontrado");
        }
    }
}
