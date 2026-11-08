using Livros.Application.Common;
using Livros.Domain.Events;
using Livros.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Livros.Application.EventHandlers;

/// <summary>
/// Handler do evento AvaliacaoCadastradaEvent: é ele que recalcula a nota
/// média do livro sempre que uma avaliação é cadastrada, mantendo o command
/// de avaliação desacoplado do cálculo da média.
/// </summary>
public class AvaliacaoCadastradaEventHandler
    : INotificationHandler<DomainEventNotification<AvaliacaoCadastradaEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AvaliacaoCadastradaEventHandler> _logger;

    public AvaliacaoCadastradaEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<AvaliacaoCadastradaEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<AvaliacaoCadastradaEvent> notification,
        CancellationToken cancellationToken)
    {
        var evento = notification.DomainEvent;

        var livro = await _unitOfWork.Livros
            .ObterPorIdComAvaliacoesAsync(evento.LivroId, cancellationToken);

        if (livro is null)
        {
            _logger.LogWarning(
                "Livro {LivroId} não encontrado ao recalcular a nota média.", evento.LivroId);
            return;
        }

        // A própria entidade recalcula e dispara NotaMediaAtualizadaEvent se mudar.
        livro.RecalcularNotaMedia();

        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Nota média de '{Titulo}' recalculada para {Media} após avaliação de nota {Nota}.",
            livro.Titulo, livro.NotaMedia, evento.Nota);
    }
}
