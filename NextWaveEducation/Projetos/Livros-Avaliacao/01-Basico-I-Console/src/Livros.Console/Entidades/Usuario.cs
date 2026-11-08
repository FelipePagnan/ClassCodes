namespace Livros.Console.Entidades;

public class Usuario
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; } = DateTime.Now;

    public List<Avaliacao> Avaliacoes { get; set; } = new();

    public int TotalAvaliacoes => Avaliacoes.Count;

    public double? NotaMediaAtribuida => Avaliacoes.Count == 0
        ? null
        : Math.Round(Avaliacoes.Average(a => a.Nota), 2);

    public override string ToString()
        => $"[{Id}] {Nome} - {Email} - {TotalAvaliacoes} avaliação(ões)";
}
