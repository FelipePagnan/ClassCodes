using Livros.Application.Common;
using Livros.Domain.Exceptions;
using Livros.Domain.Repositories;
using MediatR;

namespace Livros.Application.Avaliacoes.Queries.ListarAvaliacoesDoLivro;

public record ListarAvaliacoesDoLivroQuery(int IdLivro) : IRequest<List<AvaliacaoDto>>;

public class ListarAvaliacoesDoLivroQueryHandler
    : IRequestHandler<ListarAvaliacoesDoLivroQuery, List<AvaliacaoDto>>
{
    private readonly ILivroRepository _livroRepository;
    private readonly IAvaliacaoRepository _avaliacaoRepository;

    public ListarAvaliacoesDoLivroQueryHandler(
        ILivroRepository livroRepository,
        IAvaliacaoRepository avaliacaoRepository)
    {
        _livroRepository = livroRepository;
        _avaliacaoRepository = avaliacaoRepository;
    }

    public async Task<List<AvaliacaoDto>> Handle(
        ListarAvaliacoesDoLivroQuery request,
        CancellationToken cancellationToken)
    {
        if (await _livroRepository.ObterPorIdAsync(request.IdLivro, cancellationToken) is null)
            throw NaoEncontradoException.Para("Livro", request.IdLivro);

        var avaliacoes = await _avaliacaoRepository.ObterPorLivroAsync(request.IdLivro, cancellationToken);
        return avaliacoes.Select(AvaliacaoDto.De).ToList();
    }
}
