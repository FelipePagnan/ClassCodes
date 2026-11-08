using Livros.Application.Common;
using Livros.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Livros.Application.EventHandlers;

/// <summary>
/// Handler do evento NotaMediaAtualizadaEvent. Num cenário real poderia
/// invalidar cache, reordenar rankings ou notificar seguidores do livro.
/// </summary>
public class NotaMediaAtualizadaEventHandler
    : INotificationHandler<DomainEventNotification<NotaMediaAtualizadaEvent>>
{
    private readonly ILogger<NotaMediaAtualizadaEventHandler> _logger;

    public NotaMediaAtualizadaEventHandler(ILogger<NotaMediaAtualizadaEventHandler> logger)
        => _logger = logger;

    public Task Handle(
        DomainEventNotification<NotaMediaAtualizadaEvent> notification,
        CancellationToken cancellationToken)
    {
        var evento = notification.DomainEvent;
        var sinal = evento.Variacao >= 0 ? "+" : string.Empty;

        _logger.LogInformation(
            "Nota média de '{Titulo}' passou de {Anterior} para {Atual} ({Sinal}{Variacao}) " +
            "com {Total} avaliação(ões).",
            evento.TituloLivro, evento.NotaMediaAnterior, evento.NotaMediaAtual,
            sinal, evento.Variacao, evento.TotalAvaliacoes);

        return Task.CompletedTask;
    }
}
