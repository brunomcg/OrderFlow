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
        // 💡 REQUISITO: Não pode criar pedido sem itens
        if (request.Items == null || !request.Items.Any())
        {
            return Result<long>.Failure("Items", "Não é possível criar um pedido sem itens.");
        }

        // 1. Valida e cria a entidade Order passando também o Currency
        // 💡 AJUSTE: Adicionado request.Currency
        var orderResult = SalesOrder.Create(request.CustomerId, request.Currency);
        if (!orderResult.IsSuccess)
        {
            return Result<long>.Failure(orderResult.Field ?? nameof(request.CustomerId), orderResult.Error);
        }

        var order = orderResult.Value;

        // =======================================================================
        // 2. OTIMIZAÇÃO DE PERFORMANCE BRUTA: Busca em Bloco (Resolve Gargalo N+1)
        // =======================================================================

        // Extrai todos os IDs únicos de produtos enviados na requisição
        var productIds = request.Items
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        // Faz UMA ÚNICA viagem ao banco para trazer todos os produtos de vez.
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken); // Busca O(1) na memória RAM

        // 3. Processamento das regras de negócio em memória
        foreach (var itemInput in request.Items)
        {
            // Busca o produto diretamente no dicionário carregado (Sem novas queries ao banco)
            if (!products.TryGetValue(itemInput.ProductId, out var product))
            {
                return Result<long>.Failure(nameof(itemInput.ProductId), $"Produto com ID {itemInput.ProductId} não foi encontrado.");
            }

            // Executa a regra de negócio do Domínio: Deduzir o estoque
            var stockResult = product.DeductStock(itemInput.Quantity);
            if (!stockResult.IsSuccess)
            {
                return Result<long>.Failure(stockResult.Field ?? nameof(itemInput.Quantity), stockResult.Error);
            }

            // Executa a regra de negócio do Domínio: Adicionar o item ao pedido
            // 💡 AJUSTE CRÍTICO: Usando o product.UnitPrice (Preço do Banco) em vez de itemInput.UnitPrice
            var addItemResult = order.AddItem(product.Id, product.UnitPrice, itemInput.Quantity);
            if (!addItemResult.IsSuccess)
            {
                return Result<long>.Failure(addItemResult.Field ?? "Items", addItemResult.Error);
            }
        }

        // =======================================================================
        // 4. PERSISTÊNCIA ATÔMICA (Unit of Work nativo do DbContext)
        // =======================================================================

        // Adiciona a ordem ao contexto (o EF identifica os itens internos e gera os INSERTS corretos)
        await _context.SalesOrders.AddAsync(order, cancellationToken);

        // Dispara uma única transação no Postgres contendo:
        // - 1 INSERT na tabela sales_order
        // - X INSERTS na tabela sales_order_items
        // - X UPDATES na tabela product (ajustando o estoque)
        await _context.SaveChangesAsync(cancellationToken);

        // Retorna o ID da ordem criada com sucesso
        return Result<long>.Success(order.Id);
    }
}