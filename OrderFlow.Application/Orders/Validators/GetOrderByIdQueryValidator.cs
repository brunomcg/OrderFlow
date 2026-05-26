using FluentValidation;
using OrderFlow.Application.Orders.Queries;

namespace OrderFlow.Application.Orders.Validators;

public class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O identificador do pedido é obrigatório.")
            .GreaterThan(0).WithMessage("O identificador do pedido deve ser maior que zero.");
    }
}
