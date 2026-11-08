using Caixinhas.Application.Common;
using Caixinhas.Domain.Exceptions;
using Caixinhas.Domain.Repositories;
using MediatR;

namespace Caixinhas.Application.Transacoes.Queries.ObterTransacaoPorId;

public record ObterTransacaoPorIdQuery(int IdObjetivo, int IdTransacao) : IRequest<TransacaoDto>;

public class ObterTransacaoPorIdQueryHandler
    : IRequestHandler<ObterTransacaoPorIdQuery, TransacaoDto>
{
    private readonly ITransacaoRepository _repository;

    public ObterTransacaoPorIdQueryHandler(ITransacaoRepository repository)
        => _repository = repository;

    public async Task<TransacaoDto> Handle(
        ObterTransacaoPorIdQuery request,
        CancellationToken cancellationToken)
    {
        var transacao = await _repository.ObterPorIdAsync(
            request.IdObjetivo, request.IdTransacao, cancellationToken)
            ?? throw new NaoEncontradoException(
                $"Transação {request.IdTransacao} não encontrada no objetivo {request.IdObjetivo}.");

        return TransacaoDto.De(transacao);
    }
}
