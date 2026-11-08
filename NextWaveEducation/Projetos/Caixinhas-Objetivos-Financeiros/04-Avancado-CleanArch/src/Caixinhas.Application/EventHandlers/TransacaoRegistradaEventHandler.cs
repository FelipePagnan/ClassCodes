using Caixinhas.Application.Common;
using Caixinhas.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Caixinhas.Application.EventHandlers;

/// <summary>
/// Handler do evento TransacaoRegistradaEvent. Confere se a variação do
/// ValorTotal bate com o efeito da transação: é a validação de que o saldo
/// mantido incrementalmente continua correto.
/// </summary>
public class TransacaoRegistradaEventHandler
    : INotificationHandler<DomainEventNotification<TransacaoRegistradaEvent>>
{
    private readonly ILogger<TransacaoRegistradaEventHandler> _logger;

    public TransacaoRegistradaEventHandler(ILogger<TransacaoRegistradaEventHandler> logger)
        => _logger = logger;

    public Task Handle(
        DomainEventNotification<TransacaoRegistradaEvent> notification,
        CancellationToken cancellationToken)
    {
        var evento = notification.DomainEvent;

        var efeitoEsperado = evento.Tipo == Domain.Enums.TipoTransacao.Deposito
            ? evento.Quantidade
            : -evento.Quantidade;

        var variacaoReal = evento.ValorTotalAtual - evento.ValorTotalAnterior;

        if (variacaoReal != efeitoEsperado)
        {
            _logger.LogError(
                "Divergência no saldo de '{Titulo}': esperado {Esperado}, aplicado {Real}.",
                evento.TituloObjetivo, efeitoEsperado, variacaoReal);
        }
        else
        {
            _logger.LogInformation(
                "{Tipo} de R$ {Quantidade} em '{Titulo}' ({Data:dd/MM/yyyy}). " +
                "Saldo: R$ {Anterior} -> R$ {Atual}.",
                evento.Tipo, evento.Quantidade, evento.TituloObjetivo, evento.DataTransacao,
                evento.ValorTotalAnterior, evento.ValorTotalAtual);
        }

        return Task.CompletedTask;
    }
}
