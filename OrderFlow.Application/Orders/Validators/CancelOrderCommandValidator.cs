using FluentValidation;
using OrderFlow.Application.Orders.Commands;

namespace OrderFlow.Application.Orders.Validators;

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.Id)
           .GreaterThan(0)
           .WithMessage("O identificador do pedido de venda deve ser maior que zero.");
    }
}