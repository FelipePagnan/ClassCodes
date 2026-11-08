using Livros.Application.Common.Interfaces;
using Livros.Domain.Exceptions;
using Livros.Domain.Repositories;
using MediatR;

namespace Livros.Application.Livros.Queries.ObterCapa;

/// <summary>Devolve os bytes da capa do livro e o tipo de conteúdo detectado.</summary>
public record ObterCapaQuery(int IdLivro) : IRequest<CapaLivroDto>;

public record CapaLivroDto(byte[] Conteudo, string TipoConteudo);

public class ObterCapaQueryHandler : IRequestHandler<ObterCapaQuery, CapaLivroDto>
{
    private readonly ILivroRepository _repository;
    private readonly IValidadorDeImagem _validadorDeImagem;

    public ObterCapaQueryHandler(
        ILivroRepository repository,
        IValidadorDeImagem validadorDeImagem)
    {
        _repository = repository;
        _validadorDeImagem = validadorDeImagem;
    }

    public async Task<CapaLivroDto> Handle(ObterCapaQuery request, CancellationToken cancellationToken)
    {
        var livro = await _repository.ObterPorIdAsync(request.IdLivro, cancellationToken)
            ?? throw NaoEncontradoException.Para("Livro", request.IdLivro);

        if (!livro.TemCapa)
            throw new NaoEncontradoException($"O livro '{livro.Titulo}' não possui capa cadastrada.");

        var conteudo = livro.CapaLivro!;
        return new CapaLivroDto(conteudo, _validadorDeImagem.DetectarTipoConteudo(conteudo));
    }
}
