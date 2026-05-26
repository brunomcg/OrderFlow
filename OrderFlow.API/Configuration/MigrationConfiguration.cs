using Microsoft.EntityFrameworkCore;
using OrderFlow.Infrastructure.Data;

    public static class MigrationConfiguration
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();

                if (context.Database.IsRelational())
                {
                    context.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();

                logger.LogError(ex, "Ocorreu um erro ao aplicar as migrations automaticamente no banco de dados.");

                throw;
            }
        }
    }


