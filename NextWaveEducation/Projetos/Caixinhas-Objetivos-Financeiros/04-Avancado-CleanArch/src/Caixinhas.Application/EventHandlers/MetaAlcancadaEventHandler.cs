using Caixinhas.Application.Common;
using Caixinhas.Application.Common.Interfaces;
using Caixinhas.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Caixinhas.Application.EventHandlers;

/// <summary>
/// Handler do evento MetaAlcancadaEvent: avisa o usuário que a caixinha
/// bateu a meta.
/// </summary>
public class MetaAlcancadaEventHandler
    : INotificationHandler<DomainEventNotification<MetaAlcancadaEvent>>
{
    private readonly INotificadorDeMetas _notificador;
    private readonly ILogger<MetaAlcancadaEventHandler> _logger;

    public MetaAlcancadaEventHandler(
        INotificadorDeMetas notificador,
        ILogger<MetaAlcancadaEventHandler> logger)
    {
        _notificador = notificador;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<MetaAlcancadaEvent> notification,
        CancellationToken cancellationToken)
    {
        var evento = notification.DomainEvent;

        _logger.LogInformation(
            "META ALCANÇADA: '{Titulo}' chegou a R$ {Valor} (meta de R$ {Meta}).",
            evento.Titulo, evento.ValorTotal, evento.QuantidadeAlvo);

        await _notificador.NotificarMetaAlcancadaAsync(
            evento.Titulo,
            evento.ValorTotal,
            evento.QuantidadeAlvo,
            evento.DiasParaOPrazo,
            cancellationToken);
    }
}
