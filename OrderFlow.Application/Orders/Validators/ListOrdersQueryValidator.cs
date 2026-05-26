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
             .When(x => x.CustomerId.HasValue); // Se o seu campo for "long?"
                                                // Se for long primitivo (onde o padrão é 0 se não enviado), use: .When(x => x.CustomerId > 0);

        // 2. Valida o Status APENAS se ele for informado
        RuleFor(x => x.Status)
           .GreaterThan(0)
           .WithMessage("O identificador do status deve ser maior que zero.")
           .Must(statusId => SalesOrderStatus.List().Any(s => s.Id == statusId.Value))
           .WithMessage(x => $"O status informado ({x.Status}) é inválido. Status permitidos: {string.Join(", ", SalesOrderStatus.List().Select(s => $"{s.Id}-{s.Name}"))}.")
           .When(x => x.Status.HasValue);

        // Valida o formato da data inicial apenas se ela for preenchida
        When(x => !string.IsNullOrWhiteSpace(x.From), () =>
        {
            RuleFor(x => x.From)
                .Must(date => DateTimeOffset.TryParse(date, out _))
                .WithMessage("A data inicial (From) possui um formato inválido. Utilize o padrão AAAA-MM-DD.");
        });

        // Valida o formato da data final apenas se ela for preenchida
        When(x => !string.IsNullOrWhiteSpace(x.To), () =>
        {
            RuleFor(x => x.To)
                .Must(date => DateTimeOffset.TryParse(date, out _))
                .WithMessage("A data final (To) possui um formato inválido. Utilize o padrão AAAA-MM-DD.");
        });

        // 3. Validações de Paginação Segura (Boa prática Sênior)
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("O número da página deve ser maior que zero.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("O tamanho da página deve ser entre 1 e 100 itens.");

        // 4. Regra de Negócio para as Datas: Garante que a data inicial não seja maior que a final
        RuleFor(x => x.From)
            .Must((query, fromStr) =>
            {
                // 1. Tenta converter a string do From para DateTimeOffset
                if (!DateTimeOffset.TryParse(fromStr, out var fromDate))
                    return true; // Se falhar o formato, a regra anterior de formato já vai pegar o erro

                // 2. Tenta converter a string do To para DateTimeOffset
                if (!DateTimeOffset.TryParse(query.To, out var toDate))
                    return true; // Se o To estiver inválido ou nulo, não temos como comparar ainda

                // 3. Aplica a regra de negócio: From deve ser menor ou igual ao To
                return fromDate <= toDate;
           })
           .WithMessage("A data inicial (From) não pode ser maior que a data final (To).")
           .When(x => !string.IsNullOrWhiteSpace(x.From) && !string.IsNullOrWhiteSpace(x.To));
    }
}