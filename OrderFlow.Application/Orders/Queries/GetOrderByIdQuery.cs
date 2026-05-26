using MediatR;
using OrderFlow.Application.Orders.Queries.DTOs;
using OrderFlow.Domain.Common;

namespace OrderFlow.Application.Orders.Queries;

public record GetOrderByIdQuery(long Id) : IRequest<Result<OrderDetailsDto>>;