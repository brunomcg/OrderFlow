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
        // Método auxiliar para garantir que o banco em memória tenha os Smart Enums populados (Seed)
        private async Task SeedDatabaseAsync(DbContextOptions<ApplicationDbContext> options)
        {
            using var context = new ApplicationDbContext(options);

            // Se os status já foram adicionados nesta instância de memória, não adiciona de novo
            if (!await context.Set<SalesOrderStatus>().AnyAsync())
            {
                context.Set<SalesOrderStatus>().AddRange(SalesOrderStatus.List());
                await context.SaveChangesAsync();
            }
        }

        // Cenário de Sucesso - Cancelamento com Retorno de Estoque
        [Fact]
        public async Task Handle_DeveCancelarPedidoEDevolverEstoque_ComSucesso()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            await SeedDatabaseAsync(options);

            long orderId;

            using (var arrangeContext = new ApplicationDbContext(options))
            {
                // 1. Cria e salva o produto primeiro para gerar um ID real do banco
                var product = Product.Create("Produto A", 5m, 10).Value;
                arrangeContext.Products.Add(product);
                await arrangeContext.SaveChangesAsync();

                // 2. Agora cria o pedido referenciando o ID gerado do produto
                var order = SalesOrder.Create(1, "BRL").Value;
                var addItemResult = order.AddItem(product.Id, product.UnitPrice, 3);
                addItemResult.IsSuccess.Should().BeTrue();

                arrangeContext.SalesOrders.Add(order);
                await arrangeContext.SaveChangesAsync();

                orderId = order.Id;
            }

            // Act
            using (var actContext = new ApplicationDbContext(options))
            {
                var handler = new CancelOrderCommandHandler(actContext);
                var result = await handler.Handle(new CancelOrderCommand(orderId), CancellationToken.None);

                // Assert
                result.IsSuccess.Should().BeTrue();
            }

            // Assert: verificar estado do pedido e estoque do produto em novo contexto limpo
            using (var assertContext = new ApplicationDbContext(options))
            {
                var savedOrder = await assertContext.SalesOrders
                    .Include(o => o.SalesOrderStatus)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                savedOrder.Should().NotBeNull();
                savedOrder!.SalesOrderStatusId.Should().Be(SalesOrderStatus.Canceled.Id);

                var savedProduct = await assertContext.Products.FirstAsync();
                // estoque inicial 10 + quantidade do item 3 = 13
                savedProduct.AvailableQuantity.Should().Be(13);
            }
        }

        // Cenário de Idempotência (Pedido já Cancelado)
        [Fact]
        public async Task Handle_DeveSerIdempotente_QuandoPedidoJaEstaCancelado()
        {
            // Arrange
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

                // Cancela previamente para que o pedido já esteja em estado Canceled
                var cancelResult = order.Cancel();
                cancelResult.IsSuccess.Should().BeTrue();

                arrangeContext.SalesOrders.Add(order);
                await arrangeContext.SaveChangesAsync();

                orderId = order.Id;
            }

            // Act
            using (var actContext = new ApplicationDbContext(options))
            {
                var handler = new CancelOrderCommandHandler(actContext);
                var result = await handler.Handle(new CancelOrderCommand(orderId), CancellationToken.None);

                // Assert: O domínio impede novo cancelamento retornando sucesso de forma estável (Idempotente)
                result.IsSuccess.Should().BeTrue();
            }

            // Assert: verificar que o estoque do produto não foi alterado
            using (var assertContext = new ApplicationDbContext(options))
            {
                var savedOrder = await assertContext.SalesOrders
                    .Include(o => o.SalesOrderStatus)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                savedOrder.Should().NotBeNull();
                savedOrder!.SalesOrderStatusId.Should().Be(SalesOrderStatus.Canceled.Id);

                var savedProduct = await assertContext.Products.FirstAsync();
                // estoque original 10 deve permanecer inalterado
                savedProduct.AvailableQuantity.Should().Be(10);
            }
        }

        // Cenário de Falha - Pedido Não Encontrado
        [Fact]
        public async Task Handle_DeveRetornarFalha_QuandoPedidoNaoEncontrado()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            await SeedDatabaseAsync(options);

            using var context = new ApplicationDbContext(options);
            var handler = new CancelOrderCommandHandler(context);

            // Act
            var result = await handler.Handle(new CancelOrderCommand(999), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Field.Should().Be("Id");
            result.Error.Should().Contain("não foi encontrado");
        }
    }
}