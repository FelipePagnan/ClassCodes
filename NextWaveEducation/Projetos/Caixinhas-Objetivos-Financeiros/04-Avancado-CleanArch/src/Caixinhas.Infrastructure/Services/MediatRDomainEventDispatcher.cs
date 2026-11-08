using Caixinhas.Application.Common;
using Caixinhas.Application.Common.Interfaces;
using Caixinhas.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Caixinhas.Infrastructure.Services;

/// <summary>
/// Publica cada evento de domínio como uma notificação do MediatR,
/// embrulhando-o em DomainEventNotification&lt;T&gt; via reflexão.
/// </summary>
public class MediatRDomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IPublisher _publisher;
    private readonly ILogger<MediatRDomainEventDispatcher> _logger;

    public MediatRDomainEventDispatcher(
        IPublisher publisher,
        ILogger<MediatRDomainEventDispatcher> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task DespacharAsync(
        IEnumerable<IDomainEvent> eventos,
        CancellationToken cancellationToken = default)
    {
        foreach (var evento in eventos)
        {
            var tipoNotificacao = typeof(DomainEventNotification<>).MakeGenericType(evento.GetType());
            var notificacao = Activator.CreateInstance(tipoNotificacao, evento);

            if (notificacao is INotification tipada)
            {
                _logger.LogDebug("Despachando evento de domínio {Evento}", evento.GetType().Name);
                await _publisher.Publish(tipada, cancellationToken);
            }
        }
    }
}
