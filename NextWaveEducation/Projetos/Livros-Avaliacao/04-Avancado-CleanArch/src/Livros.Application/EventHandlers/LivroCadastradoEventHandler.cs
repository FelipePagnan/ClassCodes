using Livros.Application.Common;
using Livros.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Livros.Application.EventHandlers;

public class LivroCadastradoEventHandler
    : INotificationHandler<DomainEventNotification<LivroCadastradoEvent>>
{
    private readonly ILogger<LivroCadastradoEventHandler> _logger;

    public LivroCadastradoEventHandler(ILogger<LivroCadastradoEventHandler> logger)
        => _logger = logger;

    public Task Handle(
        DomainEventNotification<LivroCadastradoEvent> notification,
        CancellationToken cancellationToken)
    {
        var evento = notification.DomainEvent;

        _logger.LogInformation(
            "Novo livro no catálogo: '{Titulo}' de {Autor} (ISBN {Isbn}).",
            evento.Titulo, evento.Autor, evento.Isbn);

        return Task.CompletedTask;
    }
}
