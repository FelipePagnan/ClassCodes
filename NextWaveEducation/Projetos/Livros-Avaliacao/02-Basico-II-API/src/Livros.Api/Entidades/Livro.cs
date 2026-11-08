using Livros.Api.Enums;

namespace Livros.Api.Entidades;

public class Livro
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public string Editora { get; set; } = string.Empty;
    public GeneroLivroEnum Genero { get; set; }
    public int AnoDePublicacao { get; set; }
    public int QuantidadePaginas { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.Now;

    /// <summary>Média das notas, recalculada a cada avaliação cadastrada.</summary>
    public decimal NotaMedia { get; set; }

    /// <summary>Imagem da capa armazenada como bytes.</summary>
    public byte[]? CapaLivro { get; set; }

    public List<Avaliacao> Avaliacoes { get; set; } = new();

    public bool TemCapa => CapaLivro is { Length: > 0 };

    public void RecalcularNotaMedia()
    {
        NotaMedia = Avaliacoes.Count == 0
            ? 0
            : Math.Round((decimal)Avaliacoes.Average(a => a.Nota), 2);
    }
}
