using Clinica.Api.Servicos;

namespace Clinica.Api.BackgroundServices;

/// <summary>
/// HostedService que roda junto com a API e notifica os pacientes com
/// atendimento no dia seguinte.
///
/// Dois detalhes que fazem a diferença na prática:
///   - o serviço é singleton e os services são scoped, então cada rodada abre
///     o próprio escopo com IServiceScopeFactory;
///   - cada rodada é envolvida em try/catch, para que uma falha não derrube o
///     laço e deixe a clínica sem lembretes até o próximo deploy.
/// </summary>
public class LembreteBackgroundService : BackgroundService
{
    /// <summary>Espera antes da primeira rodada, para a API subir primeiro.</summary>
    private static readonly TimeSpan AtrasoInicial = TimeSpan.FromSeconds(10);

    /// <summary>Intervalo entre rodadas.</summary>
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LembreteBackgroundService> _logger;

    public LembreteBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<LembreteBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Serviço de lembretes iniciado. Primeira rodada em {Atraso}, depois a cada {Intervalo}.",
            AtrasoInicial, Intervalo);

        try
        {
            await Task.Delay(AtrasoInicial, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await ExecutarRodadaAsync();
                await Task.Delay(Intervalo, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Encerramento normal da aplicação.
            _logger.LogInformation("Serviço de lembretes encerrado.");
        }
    }

    private async Task ExecutarRodadaAsync()
    {
        try
        {
            using var escopo = _scopeFactory.CreateScope();

            var notificacaoService = escopo.ServiceProvider
                .GetRequiredService<NotificacaoService>();

            var rodada = await notificacaoService.ExecutarRodadaDeLembretesAsync();

            if (rodada.AtendimentosEncontrados == 0)
            {
                _logger.LogInformation(
                    "Rodada de lembretes: nenhum atendimento pendente para amanhã.");
                return;
            }

            _logger.LogInformation(
                "Rodada de lembretes: {Atendimentos} atendimento(s), " +
                "{Notificacoes} notificação(ões) enviada(s).",
                rodada.AtendimentosEncontrados, rodada.Enviadas.Count);
        }
        catch (Exception ex)
        {
            // Engolir aqui é proposital: o laço precisa sobreviver à falha.
            _logger.LogError(ex, "Erro na rodada de lembretes. O serviço continua ativo.");
        }
    }
}
