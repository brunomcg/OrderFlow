using MediatR;
using OrderFlow.Domain.Common;

namespace OrderFlow.Application.Orders.Commands;

public record ConfirmOrderCommand(long Id) : IRequest<Result>;