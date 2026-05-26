using Asp.Versioning;
using MediatR;
using OrderFlow.API.Configuration.Filter;
using OrderFlow.Application.Orders.Commands;
using OrderFlow.Application.Orders.Queries;
using OrderFlow.Domain.Common;

namespace OrderFlow.API.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        // 1. Define o conjunto de versões que este arquivo de endpoints gerencia
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0)) // Define a V1 (1.0)
            .Build();

        // 2. Cria o grupo base com o padrão de rota versionada {version:apiVersion}
        // Nota: O .RequireAuthorization() foi removido daqui pois o FallbackPolicy já tranca tudo!
        var group = app.MapGroup("/api/v{version:apiVersion}/orders")
            .WithApiVersionSet(versionSet)
            .WithTags("Orders");

        // 3. Mapeia os endpoints vinculando-os explicitamente à V1
        group.MapPost("/", CreateOrderAsync)
            .WithName("CreateOrder")
            .AddEndpointFilter<ValidationFilter<CreateOrderCommand>>()
            .MapToApiVersion(1, 0);

        group.MapPost("/{id}/confirm", ConfirmOrderAsync)
            .WithName("ConfirmOrder")
            .AddEndpointFilter<ValidationFilter<ConfirmOrderCommand>>()
            .MapToApiVersion(1, 0);

        group.MapPost("/{id}/cancel", CancelOrderAsync)
            .WithName("CancelOrder")
            .AddEndpointFilter<ValidationFilter<CancelOrderCommand>>()
            .MapToApiVersion(1, 0);

        group.MapGet("/{id:long}", GetOrderByIdAsync)
            .WithName("GetOrderById")
            .AddEndpointFilter<ValidationFilter<GetOrderByIdQuery>>()
            .MapToApiVersion(1, 0);

        group.MapGet("/", ListOrdersAsync)
            .WithName("ListOrders")
            .AddEndpointFilter<ValidationFilter<ListOrdersQuery>>()
            .MapToApiVersion(1, 0);
    }

    private static async Task<IResult> CreateOrderAsync(
        CreateOrderCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new
            {
                Field = result.Field,
                Error = result.Error
            });
        }

        return Results.Created($"/api/orders/{result.Value}", new { Id = result.Value });
    }

    private static async Task<IResult> ConfirmOrderAsync(
        [AsParameters] ConfirmOrderCommand command,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        // Envia o comando que já passou pelo seu FluentValidation através do filtro
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new
            {
                Field = result.Field,
                Error = result.Error
            });
        }

        // Retorna 204 NoContent para indicar sucesso em uma alteração de estado limpa
        return Results.NoContent();
    }

 
    private static async Task<IResult> CancelOrderAsync(
    [AsParameters] CancelOrderCommand command, // 💡 O .NET vai instanciar o comando usando o {id} da URL
    IMediator mediator,
    CancellationToken cancellationToken)
    {
        // Envia o comando que já passou pelo seu FluentValidation através do filtro
        var result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new
            {
                Field = result.Field,
                Error = result.Error
            });
        }

        // Retorna 204 NoContent para indicar sucesso em uma alteração de estado limpa
        return Results.NoContent();
    }

    private static async Task<IResult> GetOrderByIdAsync(
     [AsParameters] GetOrderByIdQuery query, // ◄ O .NET mapeia o {id} da URL direto para cá
     IMediator mediator,
     CancellationToken cancellationToken)
    {
        // Como a query já vem montada e validada pelo filtro, é só enviar direto!
        var result = await mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return Results.NotFound(new { Error = result.Error });
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> ListOrdersAsync(
    [AsParameters] ListOrdersQuery query, // ◄ O .NET junta todos os parâmetros da URL e monta o objeto aqui
    IMediator mediator,
    CancellationToken cancellationToken)
    {
        // 💡 Fantástico: Você elimina toda a lógica manual de "page ?? 1" e "new ListOrdersQuery(...)"!
        // Se o cliente não enviar os parâmetros, o FluentValidation barra no filtro antes de entrar aqui.

        var result = await mediator.Send(query, cancellationToken);

        return Results.Ok(result);
    }
}