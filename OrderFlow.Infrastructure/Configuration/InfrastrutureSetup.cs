using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Hosting;
using OrderFlow.Infrastructure.Data;

namespace OrderFlow.Infrastructure.Configuration;

public static class InfrastructureSetup
{
    public static IServiceCollection AddInfrastructureDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("A String de Conexão 'DefaultConnection' não foi encontrada.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(
                    connectionString,
                    postgresOptions =>
                        postgresOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                .UseSnakeCaseNamingConvention()
                .ConfigureWarnings(w =>
                    w.Ignore(RelationalEventId.PendingModelChangesWarning))
        );

        return services;
    }

    public static void ApplyInfrastructureMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            context.Database.Migrate();

            var environment = services.GetRequiredService<IHostEnvironment>();

            if (environment.IsDevelopment())
            {
                DbInitializer.Initialize(context);
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
            logger.LogError(ex,
                "Ocorreu um erro ao aplicar migrations ou popular o banco.");
            throw;
        }
    }
}