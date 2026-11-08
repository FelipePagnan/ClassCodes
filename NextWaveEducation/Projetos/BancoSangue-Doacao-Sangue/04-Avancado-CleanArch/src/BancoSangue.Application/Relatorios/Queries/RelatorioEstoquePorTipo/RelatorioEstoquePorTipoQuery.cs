using BancoSangue.Application.Common;
using BancoSangue.Domain.Repositories;
using MediatR;

namespace BancoSangue.Application.Relatorios.Queries.RelatorioEstoquePorTipo;

/// <summary>Relatório da quantidade total de sangue por tipo disponível (PLUS).</summary>
public record RelatorioEstoquePorTipoQuery : IRequest<RelatorioEstoqueDto>;

public class RelatorioEstoquePorTipoQueryHandler
    : IRequestHandler<RelatorioEstoquePorTipoQuery, RelatorioEstoqueDto>
{
    private readonly IEstoqueSangueRepository _repository;

    public RelatorioEstoquePorTipoQueryHandler(IEstoqueSangueRepository repository)
        => _repository = repository;

    public async Task<RelatorioEstoqueDto> Handle(
        RelatorioEstoquePorTipoQuery request,
        CancellationToken cancellationToken)
    {
        var estoques = await _repository.ObterTodosAsync(cancellationToken);

        var porTipo = estoques
            .Select(EstoqueDto.De)
            .OrderBy(e => e.TipoSanguineo)
            .ToList();

        return new RelatorioEstoqueDto(
            porTipo.Sum(e => e.QuantidadeML),
            porTipo.Count(e => e.EstoqueBaixo),
            porTipo);
    }
}
