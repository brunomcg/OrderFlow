using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Domain.Common;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Data;

namespace OrderFlow.Application.Orders.Handlers;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result>
{
    private readonly ApplicationDbContext _context;

    public CancelOrderCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders
            .Include(o => o.SalesOrderStatus)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order == null)
        {
            return Result.Failure(nameof(request.Id), $"Pedido de venda com ID {request.Id} não foi encontrado.");
        }

        var previousStatus = order.SalesOrderStatus;

        var cancelResult = order.Cancel();
        if (!cancelResult.IsSuccess)
        {
            return Result.Failure(cancelResult.Field ?? "Status", cancelResult.Error);
        }

        if (previousStatus.Id == SalesOrderStatus.Placed.Id || previousStatus.Id == SalesOrderStatus.Confirmed.Id)
        {
            var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            foreach (var item in order.Items)
            {
                if (products.TryGetValue(item.ProductId, out var product))
                {
                    product.ReleaseStock(item.Quantity); // Devolve a quantidade ao estoque do produto
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
