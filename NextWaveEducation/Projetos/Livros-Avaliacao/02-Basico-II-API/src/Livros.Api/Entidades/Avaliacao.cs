namespace Livros.Api.Entidades;

public class Avaliacao
{
    public const int NotaMinima = 1;
    public const int NotaMaxima = 5;

    public int Id { get; set; }
    public int Nota { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int IdUsuario { get; set; }
    public int IdLivro { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.Now;

    /// <summary>Início da leitura. Base da regra "início não pode ser maior que fim".</summary>
    public DateTime DataInicioLeitura { get; set; }

    /// <summary>Fim da leitura. Base do relatório de livros lidos por ano.</summary>
    public DateTime DataFimLeitura { get; set; }

    public Livro? Livro { get; set; }
    public Usuario? Usuario { get; set; }

    public int DiasDeLeitura => (DataFimLeitura.Date - DataInicioLeitura.Date).Days + 1;
}
