using Caixinhas.Domain.Exceptions;
using Caixinhas.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Caixinhas.Application.Objetivos.Commands.RemoverObjetivo;

/// <summary>Remoção lógica da caixinha e das transações dela.</summary>
public record RemoverObjetivoCommand(int Id) : IRequest;

public class RemoverObjetivoCommandHandler : IRequestHandler<RemoverObjetivoCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoverObjetivoCommandHandler> _logger;

    public RemoverObjetivoCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<RemoverObjetivoCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(RemoverObjetivoCommand request, CancellationToken cancellationToken)
    {
        var objetivo = await _unitOfWork.Objetivos
            .ObterPorIdComTransacoesAsync(request.Id, cancellationToken)
            ?? throw NaoEncontradoException.Para("Objetivo financeiro", request.Id);

        // A entidade decide se pode ser removida (saldo tem que estar zerado).
        objetivo.Remover();

        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Objetivo {ObjetivoId} removido (soft delete).", request.Id);
    }
}
