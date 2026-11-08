using BancoSangue.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace BancoSangue.Infrastructure.Services;

/// <summary>
/// Implementação de INotificadorEstoque que registra o alerta em log.
/// Em produção seria substituída por envio de e-mail, SMS ou publicação em fila.
/// </summary>
public class NotificadorEstoqueLog : INotificadorEstoque
{
    private readonly ILogger<NotificadorEstoqueLog> _logger;

    public NotificadorEstoqueLog(ILogger<NotificadorEstoqueLog> logger) => _logger = logger;

    public Task NotificarEstoqueBaixoAsync(
        string tipoSanguineo,
        int quantidadeAtualML,
        int quantidadeMinimaML,
        CancellationToken cancellationToken = default)
    {
        var faltam = quantidadeMinimaML - quantidadeAtualML;

        _logger.LogWarning(
            "=== ALERTA DE ESTOQUE === Tipo {Tipo}: {Atual}ml de {Minimo}ml mínimos. " +
            "Faltam {Faltam}ml. Acione a campanha de doação.",
            tipoSanguineo, quantidadeAtualML, quantidadeMinimaML, faltam);

        return Task.CompletedTask;
    }
}
