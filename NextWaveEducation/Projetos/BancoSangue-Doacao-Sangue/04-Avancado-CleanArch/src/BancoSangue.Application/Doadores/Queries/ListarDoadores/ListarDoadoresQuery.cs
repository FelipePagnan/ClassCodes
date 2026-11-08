using BancoSangue.Application.Common;
using BancoSangue.Domain.Repositories;
using MediatR;

namespace BancoSangue.Application.Doadores.Queries.ListarDoadores;

/// <summary>Query (lado de leitura do CQRS) que lista todos os doadores.</summary>
public record ListarDoadoresQuery : IRequest<List<DoadorDto>>;

public class ListarDoadoresQueryHandler : IRequestHandler<ListarDoadoresQuery, List<DoadorDto>>
{
    private readonly IDoadorRepository _repository;

    public ListarDoadoresQueryHandler(IDoadorRepository repository) => _repository = repository;

    public async Task<List<DoadorDto>> Handle(ListarDoadoresQuery request, CancellationToken cancellationToken)
    {
        var doadores = await _repository.ObterTodosAsync(cancellationToken);
        return doadores.Select(DoadorDto.De).ToList();
    }
}
