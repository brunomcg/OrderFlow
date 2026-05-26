using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using OrderFlow.API.Configuration;
using OrderFlow.API.Configuration.Middlewares;
using System.Text;

public static class ApiSetup
{
    /// <summary>
    /// Centraliza todas as injeções de dependência da camada de API.
    /// </summary>
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddControllers();

        // 💡 1. Configura o tratamento global de exceções na injeção de dependência
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // 2. Inicializa o Versionamento e Swagger
        services.AddApiVersioningAndSwagger();

        // 3. Inicializa a segurança da API movida para cá
        services.AddSecurityConfig(configuration, environment);

        return services;
    }

    /// <summary>
    /// Configura o pipeline de tratamento de erros e middlewares essenciais da API.
    /// </summary>
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        // 💡 Ativa o middleware de exceções que interceptará os erros e usará o GlobalExceptionHandler
        app.UseExceptionHandler();

        return app;
    }

    /// <summary>
    /// Configura o versionamento de endpoints e o gerador do Swagger.
    /// </summary>
    public static IServiceCollection AddApiVersioningAndSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // Conecta com o seu arquivo ConfigureSwaggerOptions.cs externo
        services.ConfigureOptions<ConfigureSwaggerOptions>();
        services.AddSwaggerGen();

        return services;
    }

    /// <summary>
    /// Monta a interface do Swagger UI mapeando as versões dinamicamente.
    /// </summary>
    public static IApplicationBuilder UseSwaggerWithVersioning(
        this IApplicationBuilder app,
        IApiVersionDescriptionProvider provider)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    description.GroupName.ToUpperInvariant());
            }
        });

        return app;
    }

    /// <summary>
    /// Configura a segurança JWT e a política de fallback de rotas (Muralha de Segurança).
    /// </summary>
    public static IServiceCollection AddSecurityConfig(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var secretKey = configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("A chave secreta do JWT não foi configurada.");
        var issuer = configuration["Jwt:Issuer"] ?? "OrderFlowLocalAuth";
        var audience = configuration["Jwt:Audience"] ?? "OrderFlowLocalApi";

        var key = Encoding.ASCII.GetBytes(secretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        // =========================================================================
        // MURALHA DE SEGURANÇA INTELIGENTE: Bloqueia tudo, mas libera o Swagger local
        // =========================================================================
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAssertion(context =>
                {
                    var httpContext = context.Resource as HttpContext;
                    if (httpContext is null) return false;

                    var path = httpContext.Request.Path.Value ?? string.Empty;

                    if (environment.IsDevelopment() && path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    return httpContext.User.Identity?.IsAuthenticated ?? false;
                })
                .Build();
        });

        return services;
    }
}