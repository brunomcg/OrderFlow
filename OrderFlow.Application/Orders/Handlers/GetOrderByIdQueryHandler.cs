using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Orders.Queries;
using OrderFlow.Application.Orders.Queries.DTOs;
using OrderFlow.Domain.Common;
using OrderFlow.Infrastructure.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OrderFlow.Application.Orders.Handlers;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailsDto>>
{
    private readonly ApplicationDbContext _context;

    public GetOrderByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<OrderDetailsDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Busca os dados brutos do banco aplicando o filtro de ID primeiro
        var order = await _context.SalesOrders
            .AsNoTracking()
            .Include(o => o.SalesOrderStatus)
            .Include(o => o.Items)
               .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        // 2. Se não encontrar, usa o método estático da classe genérica Result<T>
        if (order == null)
        {
            return Result<OrderDetailsDto>.Failure($"Pedido com ID {request.Id} não foi encontrado.");
        }

        // 3. Projeta para o DTO em memória (C# puro), evitando erros de tradução do LINQ
        var orderDetailsDto = new OrderDetailsDto(
            order.Id,
            order.CustomerId,
            order.SalesOrderStatus?.Name ?? "Indefinido",
            order.Items.Sum(i => i.UnitPrice * i.Quantity),
            order.CreatedAt,
            order.Items.Select(i => new OrderItemDto(
                i.ProductId,
                i.Product.Name,
                i.UnitPrice,
                i.Quantity,
                i.UnitPrice * i.Quantity
            )).ToList()
        );

        // 4. Retorna o sucesso usando a classe genérica
        return Result<OrderDetailsDto>.Success(orderDetailsDto);
    }
}