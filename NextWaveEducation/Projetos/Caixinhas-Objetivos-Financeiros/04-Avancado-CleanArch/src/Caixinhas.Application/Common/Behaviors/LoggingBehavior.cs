using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Caixinhas.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior que registra em log cada command/query e o tempo de execução.
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

        _logger.LogInformation("Executando {Requisicao}", nome);

        try
        {
            var resposta = await next();
            cronometro.Stop();

            _logger.LogInformation("{Requisicao} concluída em {Milissegundos}ms",
                nome, cronometro.ElapsedMilliseconds);

            return resposta;
        }
        catch (Exception ex)
        {
            cronometro.Stop();

            _logger.LogWarning("{Requisicao} falhou em {Milissegundos}ms: {Mensagem}",
                nome, cronometro.ElapsedMilliseconds, ex.Message);

            throw;
        }
    }
}
