using Clinica.Aplicacao.Notificacoes;
using MediatR;
using Microsoft.Extensions.Options;

namespace Clinica.Api.BackgroundServices;

/// <summary>Configuração do serviço de lembretes, lida de appsettings.</summary>
public class LembretesConfiguracao
{
    public const string Secao = "Lembretes";

    public int IntervaloEmMinutos { get; set; } = 30;
    public int AtrasoInicialEmSegundos { get; set; } = 10;
}

/// <summary>
/// HostedService que roda junto com a API e notifica os pacientes com
/// atendimento no dia seguinte.
///
/// No Avançado ele não tem lógica nenhuma: só manda o comando
/// ExecutarRodadaDeLembretes pelo MediatR. É o mesmo comando que o endpoint
/// manual dispara — ou seja, agendado e manual passam exatamente pelo mesmo
/// caminho, validação e log incluídos.
///
/// Três detalhes que fazem diferença na prática:
///   - o serviço é singleton e o ISender é scoped, então cada rodada abre o
///     próprio escopo com IServiceScopeFactory;
///   - cada rodada é envolvida em try/catch, para que uma falha não derrube o
///     laço e deixe a clínica sem lembretes até o próximo deploy;
///   - o intervalo vem de configuração, não fixo no código.
/// </summary>
public class LembreteBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LembretesConfiguracao _configuracao;
    private readonly ILogger<LembreteBackgroundService> _logger;

    public LembreteBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<LembretesConfiguracao> configuracao,
        ILogger<LembreteBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuracao = configuracao.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var atrasoInicial = TimeSpan.FromSeconds(
            Math.Max(1, _configuracao.AtrasoInicialEmSegundos));

        var intervalo = TimeSpan.FromMinutes(Math.Max(1, _configuracao.IntervaloEmMinutos));

        _logger.LogInformation(
            "Serviço de lembretes iniciado. Primeira rodada em {Atraso}, "
            + "depois a cada {Intervalo}.", atrasoInicial, intervalo);

        try
        {
            await Task.Delay(atrasoInicial, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await ExecutarRodadaAsync(stoppingToken);
                await Task.Delay(intervalo, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Encerramento normal da aplicação.
            _logger.LogInformation("Serviço de lembretes encerrado.");
        }
    }

    private async Task ExecutarRodadaAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var escopo = _scopeFactory.CreateScope();

            var sender = escopo.ServiceProvider.GetRequiredService<ISender>();
            var rodada = await sender.Send(new ExecutarRodadaDeLembretes(), cancellationToken);

            if (rodada.AtendimentosEncontrados == 0)
            {
                _logger.LogInformation(
                    "Rodada de lembretes: nenhum atendimento pendente para amanhã.");
                return;
            }

            _logger.LogInformation(
                "Rodada de lembretes: {Atendimentos} atendimento(s), "
                + "{Notificacoes} notificação(ões) enviada(s).",
                rodada.AtendimentosEncontrados, rodada.Enviadas.Count);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Engolir aqui é proposital: o laço precisa sobreviver à falha.
            _logger.LogError(ex, "Erro na rodada de lembretes. O serviço continua ativo.");
        }
    }
}
