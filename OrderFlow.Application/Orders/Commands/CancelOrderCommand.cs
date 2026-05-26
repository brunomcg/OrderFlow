using MediatR;
using OrderFlow.Domain.Common;

namespace OrderFlow.Application.Orders.Commands;

public record CancelOrderCommand(long Id) : IRequest<Result>;