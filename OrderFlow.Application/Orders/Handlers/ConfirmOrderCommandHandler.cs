using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Domain.Common;
using OrderFlow.Infrastructure.Data;

namespace OrderFlow.Application.Orders.Handlers;

public class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, Result>
{
    private readonly ApplicationDbContext _context;

    public ConfirmOrderCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.SalesOrders
            .Include(o => o.SalesOrderStatus)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order == null)
        {
            return Result.Failure(nameof(request.Id), $"Pedido com ID {request.Id} não foi encontrado.");
        }

        var confirmResult = order.Confirm();
        if (!confirmResult.IsSuccess)
        {
            return Result.Failure(confirmResult.Field ?? "Status", confirmResult.Error);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
