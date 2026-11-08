using Caixinhas.Domain.Common;
using MediatR;

namespace Caixinhas.Application.Common;

/// <summary>
/// Adapta um evento de domínio (IDomainEvent, puro, sem dependências) para
/// uma notificação do MediatR. É esse wrapper que permite ao domínio
/// permanecer independente de bibliotecas externas.
/// </summary>
public class DomainEventNotification<TDomainEvent> : INotification
    where TDomainEvent : IDomainEvent
{
    public DomainEventNotification(TDomainEvent domainEvent) => DomainEvent = domainEvent;

    public TDomainEvent DomainEvent { get; }
}
