using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Domain.Common;
using OrderFlow.Domain.Entities;
using OrderFlow.Infrastructure.Data;

namespace OrderFlow.Application.Orders.Handlers;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<long>>
{
    private readonly ApplicationDbContext _context;

    public CreateOrderCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<long>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Items == null || !request.Items.Any())
        {
            return Result<long>.Failure("Items", "Não é possível criar um pedido sem itens.");
        }

        var orderResult = SalesOrder.Create(request.CustomerId, request.Currency);
        if (!orderResult.IsSuccess)
        {
            return Result<long>.Failure(orderResult.Field ?? nameof(request.CustomerId), orderResult.Error);
        }

        var order = orderResult.Value;


        var productIds = request.Items
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken); // Busca O(1) na memória RAM

        foreach (var itemInput in request.Items)
        {
            if (!products.TryGetValue(itemInput.ProductId, out var product))
            {
                return Result<long>.Failure(nameof(itemInput.ProductId), $"Produto com ID {itemInput.ProductId} não foi encontrado.");
            }

            var stockResult = product.DeductStock(itemInput.Quantity);
            if (!stockResult.IsSuccess)
            {
                return Result<long>.Failure(stockResult.Field ?? nameof(itemInput.Quantity), stockResult.Error);
            }

            var addItemResult = order.AddItem(product.Id, product.UnitPrice, itemInput.Quantity);
            if (!addItemResult.IsSuccess)
            {
                return Result<long>.Failure(addItemResult.Field ?? "Items", addItemResult.Error);
            }
        }


        await _context.SalesOrders.AddAsync(order, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<long>.Success(order.Id);
    }
}
