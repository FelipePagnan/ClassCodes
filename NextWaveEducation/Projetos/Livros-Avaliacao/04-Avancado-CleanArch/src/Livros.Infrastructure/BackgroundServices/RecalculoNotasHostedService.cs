using Livros.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Livros.Infrastructure.BackgroundServices;

/// <summary>
/// HostedService que varre periodicamente o catálogo em busca de livros cuja
/// nota média persistida divergiu das avaliações (por exemplo, após uma
/// importação em massa ou uma falha no meio de um commit) e corrige.
/// </summary>
public class RecalculoNotasHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RecalculoNotasHostedService> _logger;
    private readonly TimeSpan _intervalo = TimeSpan.FromMinutes(5);

    public RecalculoNotasHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<RecalculoNotasHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Serviço de conferência de notas médias iniciado. Verificação a cada {Minutos} minuto(s).",
            _intervalo.TotalMinutes);

        // Pequeno atraso inicial para a aplicação terminar de subir.
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConferirNotasMediasAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Uma falha numa varredura não deve derrubar o serviço.
                _logger.LogError(ex, "Erro ao conferir as notas médias dos livros.");
            }

            try
            {
                await Task.Delay(_intervalo, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Serviço de conferência de notas médias encerrado.");
    }

    private async Task ConferirNotasMediasAsync(CancellationToken cancellationToken)
    {
        // BackgroundService é singleton; a Unit of Work é scoped.
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var livros = await unitOfWork.Livros.ObterTodosAsync(ct: cancellationToken);
        var corrigidos = 0;

        foreach (var livro in livros)
        {
            var esperada = livro.Avaliacoes.Count == 0
                ? 0
                : Math.Round((decimal)livro.Avaliacoes.Average(a => a.Nota), 2);

            if (livro.NotaMedia == esperada) continue;

            _logger.LogWarning(
                "Nota média de '{Titulo}' estava {Persistida} e deveria ser {Esperada}. Corrigindo.",
                livro.Titulo, livro.NotaMedia, esperada);

            livro.RecalcularNotaMedia();
            corrigidos++;
        }

        if (corrigidos == 0)
        {
            _logger.LogInformation(
                "Conferência de notas: {Total} livro(s) verificado(s), todos consistentes.",
                livros.Count);
            return;
        }

        await unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Conferência de notas: {Corrigidos} de {Total} livro(s) corrigido(s).",
            corrigidos, livros.Count);
    }
}
