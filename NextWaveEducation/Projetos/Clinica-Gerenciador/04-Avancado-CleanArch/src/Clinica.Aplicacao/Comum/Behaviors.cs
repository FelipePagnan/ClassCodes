using System.Diagnostics;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Clinica.Aplicacao.Comum;

/// <summary>
/// Valida a requisição antes de ela chegar ao handler.
///
/// É o ganho prático do CQRS com pipeline: a validação não é repetida em
/// handler nenhum, basta existir um AbstractValidator para o comando. Se não
/// houver validador registrado, o comando passa direto.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validadores;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validadores)
        => _validadores = validadores;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validadores.Any()) return await next();

        var resultados = await Task.WhenAll(
            _validadores.Select(v => v.ValidateAsync(request, cancellationToken)));

        var falhas = resultados
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        // Todas as falhas de uma vez: o cliente corrige o formulário inteiro
        // numa tentativa, em vez de descobrir um erro por requisição.
        if (falhas.Count > 0) throw new ValidationException(falhas);

        return await next();
    }
}

/// <summary>
/// Registra no log cada requisição que passa pelo pipeline, com o tempo que
/// levou. Com isso, descobrir qual endpoint está lento não exige instrumentar
/// handler por handler.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        => _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var nome = typeof(TRequest).Name;
        var cronometro = Stopwatch.StartNew();

        _logger.LogInformation("-> {Requisicao}", nome);

        try
        {
            var resposta = await next();

            cronometro.Stop();
            _logger.LogInformation(
                "<- {Requisicao} em {Milissegundos}ms", nome, cronometro.ElapsedMilliseconds);

            return resposta;
        }
        catch (Exception)
        {
            cronometro.Stop();

            // Só anota a falha e o tempo; traduzir a exceção em resposta HTTP
            // é trabalho do middleware da API.
            _logger.LogWarning(
                "x- {Requisicao} falhou em {Milissegundos}ms",
                nome, cronometro.ElapsedMilliseconds);

            throw;
        }
    }
}
