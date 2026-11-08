using BancoSangue.Application.Common;
using BancoSangue.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BancoSangue.Application.EventHandlers;

/// <summary>
/// Handler do evento DoadorCadastradoEvent. Num cenário real enviaria
/// um e-mail de boas-vindas; aqui apenas registra em log.
/// </summary>
public class DoadorCadastradoEventHandler
    : INotificationHandler<DomainEventNotification<DoadorCadastradoEvent>>
{
    private readonly ILogger<DoadorCadastradoEventHandler> _logger;

    public DoadorCadastradoEventHandler(ILogger<DoadorCadastradoEventHandler> logger)
        => _logger = logger;

    public Task Handle(
        DomainEventNotification<DoadorCadastradoEvent> notification,
        CancellationToken cancellationToken)
    {
        var evento = notification.DomainEvent;

        if (evento.AptoParaDoar)
        {
            _logger.LogInformation(
                "Novo doador apto cadastrado: {Nome} <{Email}>. Enviando boas-vindas.",
                evento.NomeCompleto, evento.Email);
        }
        else
        {
            _logger.LogInformation(
                "Novo doador cadastrado sem aptidão para doar neste momento: {Nome} <{Email}>.",
                evento.NomeCompleto, evento.Email);
        }

        return Task.CompletedTask;
    }
}
