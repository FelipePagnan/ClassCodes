using BancoSangue.Application.Common;
using BancoSangue.Domain.Repositories;
using MediatR;

namespace BancoSangue.Application.Doacoes.Queries.ListarDoacoes;

public record ListarDoacoesQuery : IRequest<List<DoacaoComDoadorDto>>;

public class ListarDoacoesQueryHandler : IRequestHandler<ListarDoacoesQuery, List<DoacaoComDoadorDto>>
{
    private readonly IDoacaoRepository _repository;

    public ListarDoacoesQueryHandler(IDoacaoRepository repository) => _repository = repository;

    public async Task<List<DoacaoComDoadorDto>> Handle(
        ListarDoacoesQuery request,
        CancellationToken cancellationToken)
    {
        var doacoes = await _repository.ObterTodasAsync(cancellationToken);
        return doacoes.Select(DoacaoComDoadorDto.De).ToList();
    }
}
