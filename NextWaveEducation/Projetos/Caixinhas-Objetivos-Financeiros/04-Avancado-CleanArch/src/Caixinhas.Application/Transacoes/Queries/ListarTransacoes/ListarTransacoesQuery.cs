using Caixinhas.Application.Common;
using Caixinhas.Domain.Exceptions;
using Caixinhas.Domain.Repositories;
using MediatR;

namespace Caixinhas.Application.Transacoes.Queries.ListarTransacoes;

public record ListarTransacoesQuery(int IdObjetivo) : IRequest<List<TransacaoDto>>;

public class ListarTransacoesQueryHandler
    : IRequestHandler<ListarTransacoesQuery, List<TransacaoDto>>
{
    private readonly IObjetivoRepository _objetivoRepository;
    private readonly ITransacaoRepository _transacaoRepository;

    public ListarTransacoesQueryHandler(
        IObjetivoRepository objetivoRepository,
        ITransacaoRepository transacaoRepository)
    {
        _objetivoRepository = objetivoRepository;
        _transacaoRepository = transacaoRepository;
    }

    public async Task<List<TransacaoDto>> Handle(
        ListarTransacoesQuery request,
        CancellationToken cancellationToken)
    {
        if (await _objetivoRepository.ObterPorIdAsync(request.IdObjetivo, cancellationToken) is null)
            throw NaoEncontradoException.Para("Objetivo financeiro", request.IdObjetivo);

        var transacoes = await _transacaoRepository
            .ObterPorObjetivoAsync(request.IdObjetivo, cancellationToken);

        return transacoes.Select(TransacaoDto.De).ToList();
    }
}
