using FluentValidation;

namespace OrderFlow.API.Configuration.Filter;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    private readonly IValidator<T>? _validator;

    public ValidationFilter(IValidator<T>? validator = null)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (_validator == null) return await next(context);

        var argument = context.Arguments.FirstOrDefault(x => x is T) as T;
        if (argument is null) return Results.BadRequest("Dados da requisição inválidos.");

        var validationResult = await _validator.ValidateAsync(argument);

        if (!validationResult.IsValid)
        {
            var errorDetails = validationResult.Errors
                .Select(e => new
                {
                    Field = e.PropertyName,
                    Error = e.ErrorMessage
                })
                .ToList();

            return Results.BadRequest(new
            {
                Message = "Um ou mais erros de validação ocorreram.",
                Errors = errorDetails
            });
        }

        return await next(context);
    }
}
