using Caixinhas.Domain.Exceptions;
using Caixinhas.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Caixinhas.Application.Transacoes.Commands.RemoverTransacao;

/// <summary>Remoção lógica da transação, ajustando o ValorTotal da caixinha.</summary>
public record RemoverTransacaoCommand(int IdObjetivo, int IdTransacao) : IRequest;

public class RemoverTransacaoCommandHandler : IRequestHandler<RemoverTransacaoCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoverTransacaoCommandHandler> _logger;

    public RemoverTransacaoCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<RemoverTransacaoCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(RemoverTransacaoCommand request, CancellationToken cancellationToken)
    {
        // O soft delete e o ajuste do saldo entram juntos no banco.
        await _unitOfWork.ExecutarEmTransacaoAsync(async ct =>
        {
            var objetivo = await _unitOfWork.Objetivos
                .ObterPorIdComTransacoesAsync(request.IdObjetivo, ct)
                ?? throw NaoEncontradoException.Para("Objetivo financeiro", request.IdObjetivo);

            objetivo.RemoverTransacao(request.IdTransacao);

            await _unitOfWork.CommitAsync(ct);

            _logger.LogInformation(
                "Transação {TransacaoId} removida de '{Titulo}'. Novo saldo: R$ {Saldo}.",
                request.IdTransacao, objetivo.Titulo, objetivo.ValorTotal);

            return true;
        }, cancellationToken);
    }
}
