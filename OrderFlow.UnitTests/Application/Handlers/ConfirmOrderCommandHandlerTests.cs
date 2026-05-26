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
    public class ConfirmOrderCommandHandlerTests
    {
        private static async Task SeedDatabaseAsync(ApplicationDbContext context)
        {
            context.AddRange(SalesOrderStatus.List());
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task Handle_DeveConfirmarPedidoPendente_ComSucesso()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            using (var arrangeContext = new ApplicationDbContext(options))
            {
                await SeedDatabaseAsync(arrangeContext);

                var order = SalesOrder.Create(1, "BRL").Value;
                arrangeContext.SalesOrders.Add(order);
                await arrangeContext.SaveChangesAsync();
            }

            using (var actContext = new ApplicationDbContext(options))
            {
                var handler = new ConfirmOrderCommandHandler(actContext);
                var order = await actContext.SalesOrders.FirstAsync();
                var result = await handler.Handle(new ConfirmOrderCommand(order.Id), CancellationToken.None);

                result.IsSuccess.Should().BeTrue();
            }

            using (var assertContext = new ApplicationDbContext(options))
            {
                var saved = await assertContext.SalesOrders.FirstAsync();
                saved.SalesOrderStatusId.Should().Be(SalesOrderStatus.Confirmed.Id);
            }
        }

        [Fact]
        public async Task Handle_DeveSerIdempotente_QuandoPedidoJaEstaConfirmed()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            using (var arrangeContext = new ApplicationDbContext(options))
            {
                await SeedDatabaseAsync(arrangeContext);

                var order = SalesOrder.Create(1, "BRL").Value;
                var confirm = order.Confirm();
                confirm.IsSuccess.Should().BeTrue();

                arrangeContext.SalesOrders.Add(order);
                await arrangeContext.SaveChangesAsync();
            }

            using (var actContext = new ApplicationDbContext(options))
            {
                var handler = new ConfirmOrderCommandHandler(actContext);
                var order = await actContext.SalesOrders.FirstAsync();
                var result = await handler.Handle(new ConfirmOrderCommand(order.Id), CancellationToken.None);

                result.IsSuccess.Should().BeTrue();
            }

            using (var assertContext = new ApplicationDbContext(options))
            {
                var saved = await assertContext.SalesOrders.FirstAsync();
                saved.SalesOrderStatusId.Should().Be(SalesOrderStatus.Confirmed.Id);
            }
        }

        [Fact]
        public async Task Handle_DeveRetornarFalha_QuandoPedidoNaoEncontrado()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            using var context = new ApplicationDbContext(options);
            await SeedDatabaseAsync(context);
            var handler = new ConfirmOrderCommandHandler(context);

            var result = await handler.Handle(new ConfirmOrderCommand(999), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("Id");
            result.Error.Should().Contain("não foi encontrado");
        }
    }
}

