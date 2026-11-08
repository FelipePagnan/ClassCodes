using Caixinhas.Application.Common.Interfaces;
using Caixinhas.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Caixinhas.Infrastructure.BackgroundServices;

/// <summary>
/// HostedService que faz duas rondas periódicas:
///
/// 1. Confere se o ValorTotal persistido de cada caixinha bate com a soma
///    das transações ativas e corrige divergências. Como o saldo é mantido
///    incrementalmente (requisito PLUS), essa conferência é a rede de
///    segurança contra qualquer gravação que tenha ficado pela metade.
///
/// 2. Avisa sobre caixinhas com prazo vencido que não bateram a meta.
/// </summary>
public class MonitorDeObjetivosHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MonitorDeObjetivosHostedService> _logger;
    private readonly TimeSpan _intervalo = TimeSpan.FromMinutes(5);

    public MonitorDeObjetivosHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<MonitorDeObjetivosHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Monitor de objetivos iniciado. Verificação a cada {Minutos} minuto(s).",
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
                await ExecutarRondaAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Uma falha numa ronda não deve derrubar o serviço.
                _logger.LogError(ex, "Erro durante a verificação dos objetivos.");
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

        _logger.LogInformation("Monitor de objetivos encerrado.");
    }

    private async Task ExecutarRondaAsync(CancellationToken cancellationToken)
    {
        // BackgroundService é singleton; a Unit of Work é scoped.
        using var scope = _scopeFactory.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var notificador = scope.ServiceProvider.GetRequiredService<INotificadorDeMetas>();

        var objetivos = await unitOfWork.Objetivos.ObterTodosAsync(ct: cancellationToken);
        var corrigidos = 0;

        foreach (var objetivo in objetivos)
        {
            // A entidade compara o ValorTotal com a soma das transações
            // e devolve a divergência encontrada.
            var divergencia = objetivo.ConferirValorTotal();

            if (divergencia != 0)
            {
                corrigidos++;

                _logger.LogWarning(
                    "Saldo de '{Titulo}' divergia em R$ {Divergencia}. Corrigido para R$ {Valor}.",
                    objetivo.Titulo, divergencia, objetivo.ValorTotal);
            }

            // Avisa sobre prazos vencidos sem a meta batida.
            if (objetivo.PrazoVencido && objetivo.DiasRestantes is { } dias)
            {
                await notificador.NotificarPrazoVencidoAsync(
                    objetivo.Titulo,
                    objetivo.ValorTotal,
                    objetivo.QuantidadeAlvo,
                    Math.Abs(dias),
                    cancellationToken);
            }
        }

        if (corrigidos == 0)
        {
            _logger.LogInformation(
                "Verificação: {Total} caixinha(s) conferida(s), todos os saldos consistentes.",
                objetivos.Count);
            return;
        }

        await unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Verificação: {Corrigidos} de {Total} caixinha(s) com saldo corrigido.",
            corrigidos, objetivos.Count);
    }
}
