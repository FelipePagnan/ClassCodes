using Livros.Application.Common;
using Livros.Domain.Enums;
using MediatR;

namespace Livros.Application.Livros.Commands.CadastrarLivro;

/// <summary>Command (lado de escrita do CQRS) para cadastrar um livro.</summary>
public record CadastrarLivroCommand : IRequest<LivroDto>
{
    public string Titulo { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public string Isbn { get; init; } = string.Empty;
    public string Autor { get; init; } = string.Empty;
    public string Editora { get; init; } = string.Empty;
    public GeneroLivroEnum Genero { get; init; }
    public int AnoDePublicacao { get; init; }
    public int QuantidadePaginas { get; init; }
}
