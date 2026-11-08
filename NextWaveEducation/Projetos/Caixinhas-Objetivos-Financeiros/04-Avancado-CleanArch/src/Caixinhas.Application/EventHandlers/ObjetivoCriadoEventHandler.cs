using Caixinhas.Application.Common;
using Caixinhas.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Caixinhas.Application.EventHandlers;

public class ObjetivoCriadoEventHandler
    : INotificationHandler<DomainEventNotification<ObjetivoCriadoEvent>>
{
    private readonly ILogger<ObjetivoCriadoEventHandler> _logger;

    public ObjetivoCriadoEventHandler(ILogger<ObjetivoCriadoEventHandler> logger)
        => _logger = logger;

    public Task Handle(
        DomainEventNotification<ObjetivoCriadoEvent> notification,
        CancellationToken cancellationToken)
    {
        var evento = notification.DomainEvent;

        var prazo = evento.Prazo is null
            ? "sem prazo"
            : evento.Prazo.Value.ToString("dd/MM/yyyy");

        _logger.LogInformation(
            "Nova caixinha: '{Titulo}' com meta de R$ {Meta} ({Prazo}).",
            evento.Titulo, evento.QuantidadeAlvo, prazo);

        return Task.CompletedTask;
    }
}
