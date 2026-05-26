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
            // Insere os Smart Enums de Status na base (tabela mapeada pelo EF mesmo sem DbSet exposto)
            context.AddRange(SalesOrderStatus.List());
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task Handle_DeveConfirmarPedidoPendente_ComSucesso()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            // Seed e criação do pedido em um contexto separado
            using (var arrangeContext = new ApplicationDbContext(options))
            {
                await SeedDatabaseAsync(arrangeContext);

                var order = SalesOrder.Create(1, "BRL").Value;
                arrangeContext.SalesOrders.Add(order);
                await arrangeContext.SaveChangesAsync();
            }

            // Act: executar o handler que confirma o pedido
            using (var actContext = new ApplicationDbContext(options))
            {
                var handler = new ConfirmOrderCommandHandler(actContext);
                var order = await actContext.SalesOrders.FirstAsync();
                var result = await handler.Handle(new ConfirmOrderCommand(order.Id), CancellationToken.None);

                // Assert: resultado do comando
                result.IsSuccess.Should().BeTrue();
            }

            // Assert: verificar em um novo contexto que o status foi atualizado para Confirmed
            using (var assertContext = new ApplicationDbContext(options))
            {
                var saved = await assertContext.SalesOrders.FirstAsync();
                saved.SalesOrderStatusId.Should().Be(SalesOrderStatus.Confirmed.Id);
            }
        }

        [Fact]
        public async Task Handle_DeveSerIdempotente_QuandoPedidoJaEstaConfirmed()
        {
            // Arrange
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

            // Act
            using (var actContext = new ApplicationDbContext(options))
            {
                var handler = new ConfirmOrderCommandHandler(actContext);
                var order = await actContext.SalesOrders.FirstAsync();
                var result = await handler.Handle(new ConfirmOrderCommand(order.Id), CancellationToken.None);

                // Assert
                result.IsSuccess.Should().BeTrue();
            }

            // Assert final: permanece Confirmed
            using (var assertContext = new ApplicationDbContext(options))
            {
                var saved = await assertContext.SalesOrders.FirstAsync();
                saved.SalesOrderStatusId.Should().Be(SalesOrderStatus.Confirmed.Id);
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

            using var context = new ApplicationDbContext(options);
            await SeedDatabaseAsync(context);
            var handler = new ConfirmOrderCommandHandler(context);

            // Act
            var result = await handler.Handle(new ConfirmOrderCommand(999), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("Id");
            result.Error.Should().Contain("não foi encontrado");
        }
    }
}
