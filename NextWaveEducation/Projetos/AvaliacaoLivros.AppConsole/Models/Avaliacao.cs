namespace AvaliacaoLivros.AppConsole.Models;

public class Avaliacao
{
    public int Id { get; set; }
    public int Nota { get; set; } // de 1 a 5
    public string Descricao { get; set; } = string.Empty;
    public int IdUsuario { get; set; }
    public int IdLivro { get; set; }
    public DateTime DataCriacao { get; set; }

    // Campos extras (não listados no enunciado, mas necessários para a regra de negócio
    // "data de início não pode ser maior que fim de leitura").
    public DateTime DataInicioLeitura { get; set; }
    public DateTime DataFimLeitura { get; set; }

    public override string ToString()
    {
        return $"Id: {Id} | Nota: {Nota}/5 | Usuário: {IdUsuario} | Livro: {IdLivro} | " +
               $"Leitura: {DataInicioLeitura:dd/MM/yyyy} a {DataFimLeitura:dd/MM/yyyy} | " +
               $"Comentário: {Descricao}";
    }
}
