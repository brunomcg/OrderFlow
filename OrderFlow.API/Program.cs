using Asp.Versioning.ApiExplorer;
using OrderFlow.API.Endpoints;
using OrderFlow.Application;
using OrderFlow.Infrastructure.Configuration;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. INJEÇÃO DE DEPENDÊNCIAS UNIFICADA POR CAMADA (IoC)
// =========================================================================
// Inicializa o Banco de Dados PostgreSQL (Camada de Infraestrutura)
builder.Services.AddInfrastructureDatabase(builder.Configuration);

// Inicializa os Handlers do MediatR e Validadores do FluentValidation (Camada de Application)
builder.Services.AddApplication();

// Inicializa a API: Controllers, Explorer, Versionamento, Swagger, Segurança e Tratamento de Erros
builder.Services.AddApiServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.Services.ApplyInfrastructureMigrations();

// =========================================================================
// 2. PIPELINE DE MIDDLEWARES (Ciclo de Vida da Requisição HTTP)
// =========================================================================

// 💡 1ª Linha do Pipeline: Captura e trata qualquer erro/exceção que aconteça abaixo
app.UseErrorHandling();

if (app.Environment.IsDevelopment())
{
    // Recupera o provedor de versões para alimentar o menu dropdown do Swagger UI
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwaggerWithVersioning(provider);
}

// Aplica as Migrations automaticamente no startup (Banco de Dados ganha vida no Docker)
app.ApplyMigrations();

app.UseHttpsRedirection();
//app.UseRouting();

// Ativa as travas de segurança na ordem correta exigida pelo ecossistema .NET
app.UseAuthentication(); // 1º Autentica (Valida o Token JWT enviado)
app.UseAuthorization();  // 2º Autoriza (Verifica as permissões da Muralha)

// =========================================================================
// 3. REGISTRO DE ENDPOINTS (Minimal APIs)
// =========================================================================
app.MapOrderEndpoints();
app.MapAuthEndpoints();

app.Run();