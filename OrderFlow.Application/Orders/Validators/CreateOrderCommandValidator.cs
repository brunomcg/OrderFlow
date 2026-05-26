using FluentValidation;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Domain.Entities;
using System.Linq;

namespace OrderFlow.Application.Orders.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("O identificador do cliente é obrigatório.")
            .GreaterThan(0).WithMessage("O identificador do cliente deve ser maior que zero.");

        // 💡 NOVA VALIDAÇÃO: Garante que a moeda foi preenchida e existe no Smart Enum Currency
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("A moeda (Currency) é obrigatória.")
            .Must(currencyCode => Currency.List().Any(c => c.Code.Equals(currencyCode?.Trim(), StringComparison.OrdinalIgnoreCase)))
            .WithMessage(x => $"A moeda informada '{x.Currency}' não é suportada. Moedas permitidas: {string.Join(", ", Currency.List().Select(c => c.Code))}.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("O pedido deve conter pelo menos um item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("O identificador do produto é obrigatório.")
                .GreaterThan(0).WithMessage("O identificador do produto deve ser maior que zero.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("A quantidade do item deve ser maior que zero.");
        });
    }
}