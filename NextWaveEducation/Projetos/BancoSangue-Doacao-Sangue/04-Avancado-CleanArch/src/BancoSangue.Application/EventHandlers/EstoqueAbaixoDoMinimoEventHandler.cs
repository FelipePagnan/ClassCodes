using BancoSangue.Application.Common;
using BancoSangue.Application.Common.Interfaces;
using BancoSangue.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BancoSangue.Application.EventHandlers;

/// <summary>
/// Handler do evento EstoqueAbaixoDoMinimoEvent: avisa quando o estoque
/// atinge a quantidade mínima definida (requisito PLUS).
/// </summary>
public class EstoqueAbaixoDoMinimoEventHandler
    : INotificationHandler<DomainEventNotification<EstoqueAbaixoDoMinimoEvent>>
{
    private readonly INotificadorEstoque _notificador;
    private readonly ILogger<EstoqueAbaixoDoMinimoEventHandler> _logger;

    public EstoqueAbaixoDoMinimoEventHandler(
        INotificadorEstoque notificador,
        ILogger<EstoqueAbaixoDoMinimoEventHandler> logger)
    {
        _notificador = notificador;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<EstoqueAbaixoDoMinimoEvent> notification,
        CancellationToken cancellationToken)
    {
        var evento = notification.DomainEvent;

        _logger.LogWarning("[ALERTA DE ESTOQUE] {Mensagem}", evento.Mensagem);

        await _notificador.NotificarEstoqueBaixoAsync(
            evento.TipoSanguineo,
            evento.QuantidadeAtualML,
            evento.QuantidadeMinimaML,
            cancellationToken);
    }
}
