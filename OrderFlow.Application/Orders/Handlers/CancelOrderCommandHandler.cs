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
        // 1. Busca o pedido trazendo os itens internos (essencial para saber o que devolver ao estoque)
        // 1. Busca o pedido trazendo os itens internos (essencial para saber o que devolver ao estoque)
        var order = await _context.SalesOrders
            .Include(o => o.SalesOrderStatus)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order == null)
        {
            return Result.Failure(nameof(request.Id), $"Pedido de venda com ID {request.Id} não foi encontrado.");
        }

        // 💡 Guardamos o objeto rico de status antes da mudança
        var previousStatus = order.SalesOrderStatus;

        // 2. Executa a regra de negócio no domínio
        var cancelResult = order.Cancel();
        if (!cancelResult.IsSuccess)
        {
            return Result.Failure(cancelResult.Field ?? "Status", cancelResult.Error);
        }

        // 3. SE O PEDIDO FOI CANCELADO AGORA: Devolve os produtos ao estoque
        // 💡 AJUSTE: Comparação direta por referência usando o Smart Enum (Pattern Matching)
        if (previousStatus.Id == SalesOrderStatus.Placed.Id || previousStatus.Id == SalesOrderStatus.Confirmed.Id)
        {
            // Otimização: Busca todos os produtos do pedido de uma vez só (Bulk Fetch)
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

        // 4. Persistência atômica das alterações (Status do pedido e estoques dos produtos)
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}