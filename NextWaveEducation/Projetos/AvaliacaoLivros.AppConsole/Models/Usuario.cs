namespace AvaliacaoLivros.AppConsole.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;

    public List<Avaliacao> Avaliacoes { get; set; } = new();

    public override string ToString()
    {
        return $"Id: {Id} | Nome: {Nome} | Email: {Email} | Avaliações feitas: {Avaliacoes.Count}";
    }
}
