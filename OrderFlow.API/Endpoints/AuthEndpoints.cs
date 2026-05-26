using Microsoft.IdentityModel.Tokens;
using OrderFlow.API.Configuration.Filter;
using OrderFlow.API.Endpoints.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OrderFlow.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var versionSet = app.NewApiVersionSet()
            .IsApiVersionNeutral() // ? Diz que este endpoint serve para QUALQUER versão
            .Build();

        app.MapPost("/api/login", (CustomLoginRequest request, IConfiguration configuration) =>
        {
            var expectedUsername = configuration["TestCredentials:Username"];
            var expectedPassword = configuration["TestCredentials:Password"];

            if (request.Username == expectedUsername && request.Password == expectedPassword)
            {
                var token = GenerateJwtToken(request.Username, "Admin", configuration);
                return Results.Ok(new { Token = token });
            }

            return Results.Unauthorized();
        })
        .WithApiVersionSet(versionSet) // ? Vincula ao comportamento neutro
        .WithTags("Auth")
        .AddEndpointFilter<ValidationFilter<CustomLoginRequest>>()
        .AllowAnonymous(); // Mantém a exceção pública na muralha de segurança
    }

    private static string GenerateJwtToken(string username, string role, IConfiguration configuration)
    {
        var secretKey = configuration["Jwt:SecretKey"]!;
        var issuer = configuration["Jwt:Issuer"]!;
        var audience = configuration["Jwt:Audience"]!;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            }),
            Expires = DateTime.UtcNow.AddHours(1), //Token expira em 1 hora, neste caso em um anbiente de coorporativo, pode-se aplicar validação com Blacklist (Híbrido) para invalidar tokens antigos 
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
