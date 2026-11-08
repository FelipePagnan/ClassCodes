namespace Livros.Console.Entidades;

public class Avaliacao
{
    /// <summary>Nota mínima permitida numa avaliação.</summary>
    public const int NotaMinima = 1;

    /// <summary>Nota máxima permitida numa avaliação.</summary>
    public const int NotaMaxima = 5;

    public int Id { get; set; }
    public int Nota { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int IdUsuario { get; set; }
    public int IdLivro { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.Now;

    /// <summary>
    /// Data em que o usuário começou a ler o livro.
    /// Necessária para a regra "início não pode ser maior que o fim".
    /// </summary>
    public DateTime DataInicioLeitura { get; set; }

    /// <summary>
    /// Data em que o usuário terminou a leitura. Também é a base do
    /// relatório de livros lidos por ano.
    /// </summary>
    public DateTime DataFimLeitura { get; set; }

    public Livro? Livro { get; set; }
    public Usuario? Usuario { get; set; }

    /// <summary>Quantidade de dias que o usuário levou para ler o livro.</summary>
    public int DiasDeLeitura => (DataFimLeitura.Date - DataInicioLeitura.Date).Days + 1;

    public string Estrelas => new string('*', Nota).PadRight(NotaMaxima, '-');

    public override string ToString()
        => $"[{Id}] {Estrelas} ({Nota}/5) - {DataInicioLeitura:dd/MM/yyyy} a {DataFimLeitura:dd/MM/yyyy}";
}
