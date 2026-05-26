using MediatR;
using OrderFlow.Application.Orders.Queries.DTOs;
using OrderFlow.Domain.Common;

namespace OrderFlow.Application.Orders.Queries;

public record ListOrdersQuery(
    long? Id,
    long? CustomerId,
    int? Status,
    string? From,
    string? To,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<OrderSummaryDto>>>;
