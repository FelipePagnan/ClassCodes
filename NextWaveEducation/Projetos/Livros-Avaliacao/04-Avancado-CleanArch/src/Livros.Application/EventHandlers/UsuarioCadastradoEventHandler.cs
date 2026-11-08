using Livros.Application.Common;
using Livros.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Livros.Application.EventHandlers;

/// <summary>
/// Handler do evento UsuarioCadastradoEvent. Num cenário real enviaria
/// um e-mail de boas-vindas.
/// </summary>
public class UsuarioCadastradoEventHandler
    : INotificationHandler<DomainEventNotification<UsuarioCadastradoEvent>>
{
    private readonly ILogger<UsuarioCadastradoEventHandler> _logger;

    public UsuarioCadastradoEventHandler(ILogger<UsuarioCadastradoEventHandler> logger)
        => _logger = logger;

    public Task Handle(
        DomainEventNotification<UsuarioCadastradoEvent> notification,
        CancellationToken cancellationToken)
    {
        var evento = notification.DomainEvent;

        _logger.LogInformation(
            "Novo leitor cadastrado: {Nome} <{Email}>. Enviando boas-vindas.",
            evento.Nome, evento.Email);

        return Task.CompletedTask;
    }
}
