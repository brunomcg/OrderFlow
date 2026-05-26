using FluentValidation;
using OrderFlow.Application.Orders.Queries;
using OrderFlow.Domain.Entities;

namespace OrderFlow.Application.Orders.Validators;

public class ListOrdersQueryValidator : AbstractValidator<ListOrdersQuery>
{
    public ListOrdersQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("O identificador do pedido de venda deve ser maior que zero.")
            .When(x => x.Id.HasValue);

        RuleFor(x => x.CustomerId)
             .GreaterThan(0)
             .WithMessage("O identificador do cliente deve ser maior que zero.")
             .When(x => x.CustomerId.HasValue); 

        RuleFor(x => x.Status)
           .GreaterThan(0)
           .WithMessage("O identificador do status deve ser maior que zero.")
           .Must(statusId => SalesOrderStatus.List().Any(s => s.Id == statusId.Value))
           .WithMessage(x => $"O status informado ({x.Status}) é inválido. Status permitidos: {string.Join(", ", SalesOrderStatus.List().Select(s => $"{s.Id}-{s.Name}"))}.")
           .When(x => x.Status.HasValue);

        When(x => !string.IsNullOrWhiteSpace(x.From), () =>
        {
            RuleFor(x => x.From)
                .Must(date => DateTimeOffset.TryParse(date, out _))
                .WithMessage("A data inicial (From) possui um formato inválido. Utilize o padrão AAAA-MM-DD.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.To), () =>
        {
            RuleFor(x => x.To)
                .Must(date => DateTimeOffset.TryParse(date, out _))
                .WithMessage("A data final (To) possui um formato inválido. Utilize o padrão AAAA-MM-DD.");
        });

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("O número da página deve ser maior que zero.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("O tamanho da página deve ser entre 1 e 100 itens.");

        RuleFor(x => x.From)
            .Must((query, fromStr) =>
            {
                if (!DateTimeOffset.TryParse(fromStr, out var fromDate))
                    return true; 

                if (!DateTimeOffset.TryParse(query.To, out var toDate))
                    return true; 

                return fromDate <= toDate;
            })
           .WithMessage("A data inicial (From) não pode ser maior que a data final (To).")
           .When(x => !string.IsNullOrWhiteSpace(x.From) && !string.IsNullOrWhiteSpace(x.To));
    }
}