using System.Text.Json;
using Clinica.Dominio.Excecoes;
using FluentValidation;

namespace Clinica.Api.Middlewares;

/// <summary>
/// Middleware de tratamento de erro. Envolve o pipeline inteiro, então
/// nenhum controller precisa de try/catch: a regra de negócio lança a
/// exceção e aqui ela vira a resposta HTTP certa.
///
/// A tradução é esta:
///   ValidationException  -> 400 com a lista de erros por campo;
///   RegraNegocioException -> 400 com uma mensagem;
///   qualquer outra        -> 500 com mensagem genérica, e o detalhe vai
///                            para o log, não para o cliente.
/// </summary>
public class TratamentoDeExcecaoMiddleware
{
    private static readonly JsonSerializerOptions OpcoesJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _proximo;
    private readonly ILogger<TratamentoDeExcecaoMiddleware> _logger;

    public TratamentoDeExcecaoMiddleware(
        RequestDelegate proximo, ILogger<TratamentoDeExcecaoMiddleware> logger)
    {
        _proximo = proximo;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _proximo(contexto);
        }
        catch (ValidationException excecao)
        {
            _logger.LogInformation(
                "Requisição inválida em {Caminho}: {Erros}",
                contexto.Request.Path,
                string.Join(" | ", excecao.Errors.Select(e => e.ErrorMessage)));

            // Agrupado por campo, que é o formato que o cliente consegue
            // usar para marcar os campos errados no formulário.
            var erros = excecao.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    grupo => grupo.Key,
                    grupo => grupo.Select(e => e.ErrorMessage).ToArray());

            await ResponderAsync(contexto, StatusCodes.Status400BadRequest, new
            {
                mensagem = "Há erros de validação na requisição.",
                erros
            });
        }
        catch (RegraNegocioException excecao)
        {
            _logger.LogInformation(
                "Regra de negócio violada em {Caminho}: {Mensagem}",
                contexto.Request.Path, excecao.Message);

            await ResponderAsync(contexto, StatusCodes.Status400BadRequest, new
            {
                mensagem = excecao.Message
            });
        }
        catch (Exception excecao)
        {
            // O detalhe fica no log: expor stack trace ao cliente é entregar
            // o mapa da aplicação.
            _logger.LogError(excecao,
                "Erro não tratado em {Metodo} {Caminho}",
                contexto.Request.Method, contexto.Request.Path);

            await ResponderAsync(contexto, StatusCodes.Status500InternalServerError, new
            {
                mensagem = "Erro inesperado ao processar a requisição.",
                rastreio = contexto.TraceIdentifier
            });
        }
    }

    private static async Task ResponderAsync(HttpContext contexto, int status, object corpo)
    {
        // Se a resposta já começou a ser escrita não há como trocar o status:
        // o melhor que se pode fazer é não piorar.
        if (contexto.Response.HasStarted) return;

        contexto.Response.Clear();
        contexto.Response.StatusCode = status;
        contexto.Response.ContentType = "application/json; charset=utf-8";

        await contexto.Response.WriteAsync(JsonSerializer.Serialize(corpo, OpcoesJson));
    }
}

/// <summary>Registro do middleware, para o Program ficar legível.</summary>
public static class TratamentoDeExcecaoMiddlewareExtensions
{
    public static IApplicationBuilder UseTratamentoDeExcecao(this IApplicationBuilder app)
        => app.UseMiddleware<TratamentoDeExcecaoMiddleware>();
}
