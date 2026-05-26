using Microsoft.EntityFrameworkCore;
using OrderFlow.Infrastructure.Data;

    public static class MigrationConfiguration
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            // Cria o escopo isolado para buscar os serviços
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                // Altere 'OrderFlowDbContext' para o nome exato da sua classe de DbContext
                var context = services.GetRequiredService<ApplicationDbContext>();

                if (context.Database.IsRelational())
                {
                    context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                // Buscando o Logger de forma genérica para a classe Program, 
                // que evita o uso complexo do typeof dentro do catch.
                var logger = services.GetRequiredService<ILogger<Program>>();

                logger.LogError(ex, "Ocorreu um erro ao aplicar as migrations automaticamente no banco de dados.");

                // Derruba a aplicação caso a migration falhe (essencial para o Docker Compose)
                throw;
            }
        }
    }

