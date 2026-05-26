using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Orders.Commands;

namespace OrderFlow.Application;

public static class ApplicationSetup
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // 1. Configura o MediatR (Ajustado para a sintaxe moderna e segura)
        services.AddMediatR(typeof(CreateOrderCommand).Assembly);

        // 2. Registra os validadores do FluentValidation 
        // 💡 Importante: Usamos 'AddValidatorsFromAssembly' estendendo a classe correta
        services.AddValidatorsFromAssembly(typeof(CreateOrderCommand).Assembly);

        return services;
    }
}