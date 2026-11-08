using BancoSangue.Application.Common;
using BancoSangue.Domain.Entities;
using BancoSangue.Domain.Events;
using BancoSangue.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BancoSangue.Application.EventHandlers;

/// <summary>
/// Handler do evento de domínio DoacaoRegistradaEvent:
/// é ele que atualiza o estoque de sangue sempre que uma doação é registrada,
/// mantendo o command de doação desacoplado do controle de estoque.
/// </summary>
public class DoacaoRegistradaEventHandler
    : INotificationHandler<DomainEventNotification<DoacaoRegistradaEvent>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DoacaoRegistradaEventHandler> _logger;

    public DoacaoRegistradaEventHandler(
        IUnitOfWork unitOfWork,
        ILogger<DoacaoRegistradaEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        DomainEventNotification<DoacaoRegistradaEvent> notification,
        CancellationToken cancellationToken)
    {
        var evento = notification.DomainEvent;

        var estoque = await _unitOfWork.EstoquesSangue
            .ObterPorTipoAsync(evento.TipoSanguineo, evento.FatorRh, cancellationToken);

        if (estoque is null)
        {
            // Primeira doação de um tipo que ainda não existe no estoque.
            estoque = new EstoqueSangue(evento.TipoSanguineo, evento.FatorRh, evento.QuantidadeML);
            estoque.VerificarEstoqueMinimo();
        }
        else
        {
            estoque.Adicionar(evento.QuantidadeML);
            _unitOfWork.EstoquesSangue.Atualizar(estoque);
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Estoque do tipo {Tipo} atualizado para {Quantidade}ml após doação de {DoadorNome}",
            estoque.Descricao, estoque.QuantidadeML, evento.NomeDoador);
    }
}
