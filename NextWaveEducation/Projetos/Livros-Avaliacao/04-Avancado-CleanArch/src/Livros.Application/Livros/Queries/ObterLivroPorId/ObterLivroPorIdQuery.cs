using Livros.Application.Common;
using Livros.Domain.Exceptions;
using Livros.Domain.Repositories;
using MediatR;

namespace Livros.Application.Livros.Queries.ObterLivroPorId;

/// <summary>Detalhes do livro com avaliações e distribuição de notas.</summary>
public record ObterLivroPorIdQuery(int Id) : IRequest<LivroDetalheDto>;

public class ObterLivroPorIdQueryHandler : IRequestHandler<ObterLivroPorIdQuery, LivroDetalheDto>
{
    private readonly ILivroRepository _repository;

    public ObterLivroPorIdQueryHandler(ILivroRepository repository) => _repository = repository;

    public async Task<LivroDetalheDto> Handle(
        ObterLivroPorIdQuery request,
        CancellationToken cancellationToken)
    {
        var livro = await _repository.ObterPorIdComAvaliacoesAsync(request.Id, cancellationToken)
            ?? throw NaoEncontradoException.Para("Livro", request.Id);

        return LivroDetalheDto.De(livro);
    }
}
