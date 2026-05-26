using Microsoft.AspNetCore.Diagnostics;

namespace OrderFlow.API.Configuration.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Intercepta especificamente falhas de requisição malformada (como o erro de bind do long)
            if (exception is BadHttpRequestException)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                await httpContext.Response.WriteAsJsonAsync(new
                {
                    Field = "QueryString/Body",
                    Error = "Um ou mais parâmetros enviados na requisição possuem um formato inválido ou malformado."
                }, cancellationToken);

                return true; // Erro tratado com sucesso
            }

            // Para qualquer outro erro interno (500) que possa acontecer na API
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(new
            {
                Error = "Ocorreu um erro interno inesperado no servidor."
            }, cancellationToken);

            return true;
        }
    }
}
