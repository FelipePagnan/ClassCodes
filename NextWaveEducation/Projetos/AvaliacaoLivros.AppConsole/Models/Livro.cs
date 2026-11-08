namespace AvaliacaoLivros.AppConsole.Models;

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
    public DateTime DataCriacao { get; set; }

    /// <summary>Calculada automaticamente a cada nova avaliação (PLUS).</summary>
    public decimal NotaMedia { get; set; }

    /// <summary>Armazena a capa do livro como bytes (PLUS).</summary>
    public byte[]? CapaLivro { get; set; }

    public List<Avaliacao> Avaliacoes { get; set; } = new();

    public override string ToString()
    {
        return $"Id: {Id} | Título: {Titulo} | Autor: {Autor} | ISBN: {ISBN} | " +
               $"Editora: {Editora} | Gênero: {Genero} | Ano: {AnoDePublicacao} | " +
               $"Páginas: {QuantidadePaginas} | Nota Média: {NotaMedia:0.0}/5" +
               (CapaLivro is not null ? " | [Possui capa cadastrada]" : "");
    }
}
