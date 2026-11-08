namespace Livros.Intermediario.Dominio.Interfaces;

/// <summary>
/// Abstração da API externa de consulta de livros (Google Books).
/// </summary>
public interface IConsultaLivroExternoService
{
    Task<LivroExternoDto?> ConsultarPorIsbnAsync(string isbn, CancellationToken cancellationToken = default);
    Task<List<LivroExternoDto>> ConsultarPorTermoAsync(string termo, CancellationToken cancellationToken = default);
}

/// <summary>Dados de um livro retornados pela API externa.</summary>
public record LivroExternoDto(
    string Titulo,
    string Descricao,
    string Isbn,
    string Autor,
    string Editora,
    string? Categoria,
    int AnoDePublicacao,
    int QuantidadePaginas);
