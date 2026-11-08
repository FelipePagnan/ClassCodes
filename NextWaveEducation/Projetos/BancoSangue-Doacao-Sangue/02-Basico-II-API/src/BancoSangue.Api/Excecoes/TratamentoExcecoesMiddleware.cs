using System.Net;
using System.Text.Json;
using BancoSangue.Api.Dtos;

namespace BancoSangue.Api.Excecoes;

/// <summary>
/// Middleware simples para captura de exceções de regra de negócio (nível Básico II).
/// Uma versão mais robusta aparece no nível Intermediário.
/// </summary>
public class TratamentoExcecoesMiddleware
{
    private readonly RequestDelegate _next;

    public TratamentoExcecoesMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (RegraNegocioException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ErroDto(ex.Message)));
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ErroDto($"Erro interno: {ex.Message}")));
        }
    }
}
