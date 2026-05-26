using Asp.Versioning.ApiExplorer;
using OrderFlow.API.Endpoints;
using OrderFlow.Application;
using OrderFlow.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureDatabase(builder.Configuration);

builder.Services.AddApplication();

builder.Services.AddApiServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.Services.ApplyInfrastructureMigrations();


app.UseErrorHandling();

if (app.Environment.IsDevelopment())
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwaggerWithVersioning(provider);
}

app.ApplyMigrations();

app.UseHttpsRedirection();

app.UseAuthentication(); // 1º Autentica (Valida o Token JWT enviado)
app.UseAuthorization();  // 2º Autoriza (Verifica as permissões da Muralha)

app.MapOrderEndpoints();
app.MapAuthEndpoints();

app.Run();
