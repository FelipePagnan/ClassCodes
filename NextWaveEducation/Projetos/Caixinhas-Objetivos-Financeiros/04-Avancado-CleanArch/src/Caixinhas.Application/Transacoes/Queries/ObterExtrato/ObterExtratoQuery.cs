using Caixinhas.Application.Common;
using Caixinhas.Domain.Exceptions;
using Caixinhas.Domain.Repositories;
using MediatR;

namespace Caixinhas.Application.Transacoes.Queries.ObterExtrato;

/// <summary>Extrato da caixinha com o saldo corrido após cada movimentação.</summary>
public record ObterExtratoQuery(int IdObjetivo) : IRequest<List<LinhaExtratoDto>>;

public class ObterExtratoQueryHandler : IRequestHandler<ObterExtratoQuery, List<LinhaExtratoDto>>
{
    private readonly IObjetivoRepository _repository;

    public ObterExtratoQueryHandler(IObjetivoRepository repository) => _repository = repository;

    public async Task<List<LinhaExtratoDto>> Handle(
        ObterExtratoQuery request,
        CancellationToken cancellationToken)
    {
        var objetivo = await _repository
            .ObterPorIdComTransacoesAsync(request.IdObjetivo, cancellationToken)
            ?? throw NaoEncontradoException.Para("Objetivo financeiro", request.IdObjetivo);

        var saldo = 0m;

        return objetivo.TransacoesAtivas
            .OrderBy(t => t.DataTransacao)
            .ThenBy(t => t.Id)
            .Select(t =>
            {
                saldo += t.Efeito;
                return new LinhaExtratoDto(
                    t.Id, t.DataTransacao, t.Tipo.ToString(), t.Quantidade, saldo);
            })
            .ToList();
    }
}
