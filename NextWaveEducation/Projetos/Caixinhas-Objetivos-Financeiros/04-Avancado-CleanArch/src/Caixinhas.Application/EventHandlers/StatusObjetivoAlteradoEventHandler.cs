using Caixinhas.Application.Common;
using Caixinhas.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Caixinhas.Application.EventHandlers;

public class StatusObjetivoAlteradoEventHandler
    : INotificationHandler<DomainEventNotification<StatusObjetivoAlteradoEvent>>
{
    private readonly ILogger<StatusObjetivoAlteradoEventHandler> _logger;

    public StatusObjetivoAlteradoEventHandler(
        ILogger<StatusObjetivoAlteradoEventHandler> logger) => _logger = logger;

    public Task Handle(
        DomainEventNotification<StatusObjetivoAlteradoEvent> notification,
        CancellationToken cancellationToken)
    {
        var evento = notification.DomainEvent;

        _logger.LogInformation(
            "Objetivo '{Titulo}' mudou de {Anterior} para {Atual}.",
            evento.Titulo, evento.StatusAnterior, evento.StatusAtual);

        return Task.CompletedTask;
    }
}
