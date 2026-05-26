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
        var query = _context.SalesOrders
            .AsNoTracking()
            .AsQueryable();

        if (request.Id.HasValue)
        {
            query = query.Where(o => o.Id == request.Id.Value);
        }

        if (request.CustomerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == request.CustomerId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(o => o.SalesOrderStatusId == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.From))
        {
            var fromDate = DateTimeOffset.Parse(request.From).ToUniversalTime();
            query = query.Where(o => o.CreatedAt >= fromDate);
        }

        if (!string.IsNullOrWhiteSpace(request.To))
        {
            var toDate = DateTimeOffset.Parse(request.To).Date.AddDays(1).AddTicks(-1);
            var toDateUtc = new DateTimeOffset(toDate).ToUniversalTime();

            query = query.Where(o => o.CreatedAt <= toDateUtc);
        }

        var totalCount = await query.CountAsync(cancellationToken);

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

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var pagedResult = new PagedResult<OrderSummaryDto>(items, request.Page, request.PageSize, totalCount, totalPages);

        return Result<PagedResult<OrderSummaryDto>>.Success(pagedResult);
    }
}
