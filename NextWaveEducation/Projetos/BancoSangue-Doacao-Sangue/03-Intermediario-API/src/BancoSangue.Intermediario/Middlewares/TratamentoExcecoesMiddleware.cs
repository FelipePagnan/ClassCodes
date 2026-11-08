using System.Net;
using System.Text.Json;
using BancoSangue.Intermediario.Dominio.Excecoes;
using FluentValidation;

namespace BancoSangue.Intermediario.Middlewares;

/// <summary>
/// Middleware central de tratamento de exceções.
/// Converte exceções do domínio em respostas HTTP padronizadas.
/// </summary>
public class TratamentoExcecoesMiddleware
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<TratamentoExcecoesMiddleware> _logger;

    public TratamentoExcecoesMiddleware(RequestDelegate next, ILogger<TratamentoExcecoesMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Falha de validação.");
            await EscreverRespostaAsync(context, HttpStatusCode.BadRequest, new RespostaErro
            {
                Titulo = "Erro de validação",
                Erros = ex.Errors.Select(e => e.ErrorMessage).ToList()
            });
        }
        catch (RegraNegocioException ex)
        {
            _logger.LogWarning("Regra de negócio violada: {Mensagem}", ex.Message);
            await EscreverRespostaAsync(context, HttpStatusCode.BadRequest, new RespostaErro
            {
                Titulo = "Regra de negócio violada",
                Erros = new List<string> { ex.Message }
            });
        }
        catch (NaoEncontradoException ex)
        {
            _logger.LogWarning("Recurso não encontrado: {Mensagem}", ex.Message);
            await EscreverRespostaAsync(context, HttpStatusCode.NotFound, new RespostaErro
            {
                Titulo = "Recurso não encontrado",
                Erros = new List<string> { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado.");
            await EscreverRespostaAsync(context, HttpStatusCode.InternalServerError, new RespostaErro
            {
                Titulo = "Erro interno do servidor",
                Erros = new List<string> { "Ocorreu um erro inesperado. Tente novamente mais tarde." }
            });
        }
    }

    private static async Task EscreverRespostaAsync(HttpContext context, HttpStatusCode status, RespostaErro corpo)
    {
        if (context.Response.HasStarted) return;

        context.Response.Clear();
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = (int)status;
        corpo.Status = (int)status;

        await context.Response.WriteAsync(JsonSerializer.Serialize(corpo, _jsonOptions));
    }
}

public class RespostaErro
{
    public string Titulo { get; set; } = string.Empty;
    public int Status { get; set; }
    public List<string> Erros { get; set; } = new();
}
