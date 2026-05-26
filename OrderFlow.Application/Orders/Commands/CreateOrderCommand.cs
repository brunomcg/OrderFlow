using MediatR;
using OrderFlow.Domain.Common;

namespace OrderFlow.Application.Orders.Commands;

public record CreateOrderCommand(
    long CustomerId,
    string Currency,
    List<OrderItemInput> Items) : IRequest<Result<long>>; // Retorna o ID do pedido criado

public record OrderItemInput(
    long ProductId,
    int Quantity);
