using Livros.Application.Common;
using Livros.Domain.Repositories;
using MediatR;

namespace Livros.Application.Livros.Queries.ListarLivros;

/// <summary>Query (lado de leitura do CQRS) que lista os livros.</summary>
public record ListarLivrosQuery(string? Busca = null) : IRequest<List<LivroDto>>;

public class ListarLivrosQueryHandler : IRequestHandler<ListarLivrosQuery, List<LivroDto>>
{
    private readonly ILivroRepository _repository;

    public ListarLivrosQueryHandler(ILivroRepository repository) => _repository = repository;

    public async Task<List<LivroDto>> Handle(
        ListarLivrosQuery request,
        CancellationToken cancellationToken)
    {
        var livros = await _repository.ObterTodosAsync(request.Busca, cancellationToken);
        return livros.Select(LivroDto.De).ToList();
    }
}
