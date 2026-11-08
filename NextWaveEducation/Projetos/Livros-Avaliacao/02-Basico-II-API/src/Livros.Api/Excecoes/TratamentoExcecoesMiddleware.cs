using System.Net;
using System.Text.Json;
using Livros.Api.Dtos;

namespace Livros.Api.Excecoes;

/// <summary>
/// Middleware simples de tratamento de exceções (nível Básico II).
/// Uma versão mais completa aparece no nível Intermediário.
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
            await ResponderAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (NaoEncontradoException ex)
        {
            await ResponderAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            await ResponderAsync(context, HttpStatusCode.InternalServerError, $"Erro interno: {ex.Message}");
        }
    }

    private static async Task ResponderAsync(HttpContext context, HttpStatusCode status, string mensagem)
    {
        if (context.Response.HasStarted) return;

        context.Response.Clear();
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = (int)status;

        await context.Response.WriteAsync(JsonSerializer.Serialize(new ErroDto(mensagem)));
    }
}
