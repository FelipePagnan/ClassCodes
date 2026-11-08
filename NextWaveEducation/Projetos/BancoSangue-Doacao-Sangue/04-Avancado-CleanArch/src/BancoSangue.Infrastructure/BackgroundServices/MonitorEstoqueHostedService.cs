using BancoSangue.Application.Common.Interfaces;
using BancoSangue.Domain.Entities;
using BancoSangue.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BancoSangue.Infrastructure.BackgroundServices;

/// <summary>
/// HostedService que varre periodicamente o estoque e avisa sobre os tipos
/// sanguíneos abaixo da quantidade mínima definida (requisito PLUS).
/// </summary>
public class MonitorEstoqueHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MonitorEstoqueHostedService> _logger;
    private readonly TimeSpan _intervalo = TimeSpan.FromMinutes(1);

    public MonitorEstoqueHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<MonitorEstoqueHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Monitor de estoque iniciado. Verificação a cada {Minutos} minuto(s).",
            _intervalo.TotalMinutes);

        // Pequeno atraso inicial para a aplicação terminar de subir.
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await VerificarEstoqueAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Uma falha numa varredura não deve derrubar o serviço.
                _logger.LogError(ex, "Erro ao verificar o estoque de sangue.");
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

        _logger.LogInformation("Monitor de estoque encerrado.");
    }

    private async Task VerificarEstoqueAsync(CancellationToken cancellationToken)
    {
        // BackgroundService é singleton; os repositórios são scoped.
        using var scope = _scopeFactory.CreateScope();

        var repositorio = scope.ServiceProvider.GetRequiredService<IEstoqueSangueRepository>();
        var notificador = scope.ServiceProvider.GetRequiredService<INotificadorEstoque>();

        var abaixoDoMinimo = await repositorio.ObterAbaixoDoMinimoAsync(cancellationToken);

        if (abaixoDoMinimo.Count == 0)
        {
            _logger.LogInformation("Verificação de estoque: todos os tipos estão acima do mínimo.");
            return;
        }

        _logger.LogWarning(
            "Verificação de estoque: {Quantidade} tipo(s) abaixo do mínimo.",
            abaixoDoMinimo.Count);

        foreach (var estoque in abaixoDoMinimo)
        {
            await notificador.NotificarEstoqueBaixoAsync(
                estoque.Descricao,
                estoque.QuantidadeML,
                EstoqueSangue.QuantidadeMinimaML,
                cancellationToken);
        }
    }
}
