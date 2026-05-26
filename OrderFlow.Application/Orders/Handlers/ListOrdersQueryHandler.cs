using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Orders.Queries;
using OrderFlow.Application.Orders.Queries.DTOs;
using OrderFlow.Domain.Common;
using OrderFlow.Infrastructure.Data;

namespace OrderFlow.Application.Orders.Handlers;

public class ListOrdersQueryHandler : IRequestHandler<ListOrdersQuery, Result<PagedResult<OrderSummaryDto>>>
{
    private readonly ApplicationDbContext _context;

    public ListOrdersQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<OrderSummaryDto>>> Handle(ListOrdersQuery request, CancellationToken cancellationToken)
    {
        // 1. Criamos a base da query sem executar nada no banco ainda (Read-Only otimizado)
        var query = _context.SalesOrders
            .AsNoTracking()
            .AsQueryable();

        if (request.Id.HasValue)
        {
            query = query.Where(o => o.Id == request.Id.Value);
        }

        // 2. Aplicação dos Filtros Dinâmicos
        if (request.CustomerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == request.CustomerId.Value);
        }

        // 💡 OTIMIZAÇÃO: Filtra diretamente pela FK física (SalesOrderStatusId) evitando JOINs no WHERE
        if (request.Status.HasValue)
        {
            query = query.Where(o => o.SalesOrderStatusId == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.From))
        {
            // 💡 SOLUÇÃO POSTGRES: Força a conversão para o fuso UTC exigido pelo driver Npgsql
            var fromDate = DateTimeOffset.Parse(request.From).ToUniversalTime();
            query = query.Where(o => o.CreatedAt >= fromDate);
        }

        if (!string.IsNullOrWhiteSpace(request.To))
        {
            // 💡 SOLUÇÃO POSTGRES + NEGÓCIO: Estica o filtro até o final do dia (23:59:59) e joga para UTC
            var toDate = DateTimeOffset.Parse(request.To).Date.AddDays(1).AddTicks(-1);
            var toDateUtc = new DateTimeOffset(toDate).ToUniversalTime();

            query = query.Where(o => o.CreatedAt <= toDateUtc);
        }

        // 3. Busca o Total de registros que batem com o filtro (essencial para a paginação)
        var totalCount = await query.CountAsync(cancellationToken);

        // 4. Aplica a paginação (Skip/Take) e projeta direto para o DTO resumido em uma ÚNICA query
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderSummaryDto(
                o.Id,
                o.CustomerId,
                o.SalesOrderStatus != null ? o.SalesOrderStatus.Name : "Indefinido",
                o.Items.Sum(i => i.UnitPrice * i.Quantity), // Agregação feita direto no banco
                o.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        // 5. Calcula o total de páginas
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var pagedResult = new PagedResult<OrderSummaryDto>(items, request.Page, request.PageSize, totalCount, totalPages);

        // 6. Retorna envelopado no seu objeto Result de Sucesso genérico
        return Result<PagedResult<OrderSummaryDto>>.Success(pagedResult);
    }
}