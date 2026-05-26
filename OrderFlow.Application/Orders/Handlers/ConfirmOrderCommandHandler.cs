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
        // 1. Busca o pedido incluindo o Smart Enum do Status para validação
        var order = await _context.SalesOrders
            .Include(o => o.SalesOrderStatus)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);

        if (order == null)
        {
            return Result.Failure(nameof(request.Id), $"Pedido com ID {request.Id} não foi encontrado.");
        }

        // 2. Executa a regra de negócio e idempotência no coração do Domínio
        var confirmResult = order.Confirm();
        if (!confirmResult.IsSuccess)
        {
            return Result.Failure(confirmResult.Field ?? "Status", confirmResult.Error);
        }

        // 3. Persiste a alteração de estado de forma atômica no banco
        // Se o pedido já estava confirmado, o EF Core é inteligente e não disparará nenhum UPDATE no banco.
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}