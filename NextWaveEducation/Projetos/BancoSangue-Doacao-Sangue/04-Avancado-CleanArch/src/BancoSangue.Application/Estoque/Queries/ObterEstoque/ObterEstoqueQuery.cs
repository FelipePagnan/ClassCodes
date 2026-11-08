using BancoSangue.Application.Common;
using BancoSangue.Domain.Repositories;
using MediatR;

namespace BancoSangue.Application.Estoque.Queries.ObterEstoque;

/// <summary>Lista o estoque. SomenteAbaixoDoMinimo filtra apenas os tipos em alerta.</summary>
public record ObterEstoqueQuery(bool SomenteAbaixoDoMinimo = false) : IRequest<List<EstoqueDto>>;

public class ObterEstoqueQueryHandler : IRequestHandler<ObterEstoqueQuery, List<EstoqueDto>>
{
    private readonly IEstoqueSangueRepository _repository;

    public ObterEstoqueQueryHandler(IEstoqueSangueRepository repository) => _repository = repository;

    public async Task<List<EstoqueDto>> Handle(ObterEstoqueQuery request, CancellationToken cancellationToken)
    {
        var estoques = request.SomenteAbaixoDoMinimo
            ? await _repository.ObterAbaixoDoMinimoAsync(cancellationToken)
            : await _repository.ObterTodosAsync(cancellationToken);

        return estoques
            .Select(EstoqueDto.De)
            .OrderBy(e => e.TipoSanguineo)
            .ToList();
    }
}
