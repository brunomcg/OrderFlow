using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Orders.Handlers;
using OrderFlow.Application.Orders.Queries;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Data;

namespace OrderFlow.UnitTests.Application.Handlers
{
    public class ListOrdersQueryHandlerTests
    {
        private static async Task SeedDatabaseAsync(ApplicationDbContext context)
        {
            if (!await context.Set<SalesOrderStatus>().AnyAsync())
            {
                context.AddRange(SalesOrderStatus.List());
                await context.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task Handle_DeveRetornarPaginacaoCorreta_QuandoFiltrosVazios()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            using (var arrangeContext = new ApplicationDbContext(options))
            {
                await SeedDatabaseAsync(arrangeContext);

                var o1 = SalesOrder.Create(1, "BRL").Value;
                var o2 = SalesOrder.Create(2, "BRL").Value;
                var o3 = SalesOrder.Create(3, "BRL").Value;

                arrangeContext.SalesOrders.AddRange(o1, o2, o3);
                await arrangeContext.SaveChangesAsync();

                // Ajuste: usando .UtcDateTime para compatibilidade com o tipo DateTime da entidade
                o1.GetType().GetProperty("CreatedAt")?.SetValue(o1, DateTimeOffset.UtcNow.AddHours(-3).UtcDateTime);
                o2.GetType().GetProperty("CreatedAt")?.SetValue(o2, DateTimeOffset.UtcNow.AddHours(-2).UtcDateTime);
                o3.GetType().GetProperty("CreatedAt")?.SetValue(o3, DateTimeOffset.UtcNow.AddHours(-1).UtcDateTime);

                await arrangeContext.SaveChangesAsync();
            }

            // Act
            using (var actContext = new ApplicationDbContext(options))
            {
                var handler = new ListOrdersQueryHandler(actContext);
                var query = new ListOrdersQuery(null, null, null, null, null, Page: 1, PageSize: 2);
                var result = await handler.Handle(query, CancellationToken.None);

                // Assert
                result.IsSuccess.Should().BeTrue();
                var paged = result.Value;
                paged.Items.Should().HaveCount(2);
                paged.TotalCount.Should().Be(3);
                paged.TotalPages.Should().Be(2);

                var first = paged.Items.First();
                var last = paged.Items.Skip(1).First();
                first.CreatedAt.Should().BeOnOrAfter(last.CreatedAt);
            }
        }

        [Fact]
        public async Task Handle_DeveFiltrarPorCustomerIdEStatus()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            using (var arrangeContext = new ApplicationDbContext(options))
            {
                await SeedDatabaseAsync(arrangeContext);

                var o1 = SalesOrder.Create(1, "BRL").Value;
                var o2 = SalesOrder.Create(1, "BRL").Value;
                var o3 = SalesOrder.Create(2, "BRL").Value;

                o2.Confirm();

                arrangeContext.SalesOrders.AddRange(o1, o2, o3);
                await arrangeContext.SaveChangesAsync();
            }

            using (var actContext = new ApplicationDbContext(options))
            {
                var handler = new ListOrdersQueryHandler(actContext);
                var query = new ListOrdersQuery(Id: null, CustomerId: 1, Status: SalesOrderStatus.Placed.Id, From: null, To: null, Page: 1, PageSize: 10);
                var result = await handler.Handle(query, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();
                var paged = result.Value;
                paged.Items.Should().HaveCount(1);
                paged.TotalCount.Should().Be(1);
                paged.Items.First().CustomerId.Should().Be(1);
                paged.Items.First().Status.Should().Be(SalesOrderStatus.Placed.Name);
            }
        }

        [Fact]
        public async Task Handle_DeveFiltrarPorIntervaloDeDatas()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            var dt1 = DateTimeOffset.Parse("2026-05-01T10:00:00+00:00");
            var dt2 = DateTimeOffset.Parse("2026-05-15T14:00:00+00:00");
            var dt3 = DateTimeOffset.Parse("2026-05-25T18:00:00+00:00");

            using (var arrangeContext = new ApplicationDbContext(options))
            {
                await SeedDatabaseAsync(arrangeContext);

                var o1 = SalesOrder.Create(1, "BRL").Value;
                var o2 = SalesOrder.Create(1, "BRL").Value;
                var o3 = SalesOrder.Create(1, "BRL").Value;

                arrangeContext.SalesOrders.AddRange(o1, o2, o3);
                await arrangeContext.SaveChangesAsync();

                // Ajuste: usando .UtcDateTime para garantir compatibilidade
                o1.GetType().GetProperty("CreatedAt")?.SetValue(o1, dt1.UtcDateTime);
                o2.GetType().GetProperty("CreatedAt")?.SetValue(o2, dt2.UtcDateTime);
                o3.GetType().GetProperty("CreatedAt")?.SetValue(o3, dt3.UtcDateTime);

                await arrangeContext.SaveChangesAsync();
            }

            using (var actContext = new ApplicationDbContext(options))
            {
                var handler = new ListOrdersQueryHandler(actContext);
                var query = new ListOrdersQuery(null, null, null, "2026-05-10", "2026-05-20", 1, 10);
                var result = await handler.Handle(query, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();
                var paged = result.Value;
                paged.Items.Should().HaveCount(1);

                // Assert simplificada com o valor direto de .UtcDateTime
                paged.Items.First().CreatedAt.Should().Be(dt2.UtcDateTime);
            }
        }

        [Fact]
        public async Task Handle_DeveFiltrarPorIdEspecifico()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            long idToQuery;

            using (var arrangeContext = new ApplicationDbContext(options))
            {
                await SeedDatabaseAsync(arrangeContext);

                var o1 = SalesOrder.Create(1, "BRL").Value;
                var o2 = SalesOrder.Create(2, "BRL").Value;

                arrangeContext.SalesOrders.AddRange(o1, o2);
                await arrangeContext.SaveChangesAsync();

                idToQuery = o1.Id;
            }

            using (var actContext = new ApplicationDbContext(options))
            {
                var handler = new ListOrdersQueryHandler(actContext);
                var query = new ListOrdersQuery(Id: idToQuery, CustomerId: null, Status: null, From: null, To: null, Page: 1, PageSize: 10);
                var result = await handler.Handle(query, CancellationToken.None);

                result.IsSuccess.Should().BeTrue();
                var paged = result.Value;
                paged.Items.Should().HaveCount(1);
                paged.Items.First().Id.Should().Be(idToQuery);
            }
        }
    }
}