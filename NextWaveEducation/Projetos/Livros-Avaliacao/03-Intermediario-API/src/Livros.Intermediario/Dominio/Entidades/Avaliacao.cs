using Livros.Intermediario.Dominio.Excecoes;

namespace Livros.Intermediario.Dominio.Entidades;

public class Avaliacao : EntidadeBase
{
    /// <summary>Nota mínima permitida.</summary>
    public const int NotaMinima = 1;

    /// <summary>Nota máxima permitida.</summary>
    public const int NotaMaxima = 5;

    // Construtor usado pelo EF Core.
    protected Avaliacao() { }

    public Avaliacao(
        int idLivro,
        int idUsuario,
        int nota,
        string descricao,
        DateTime dataInicioLeitura,
        DateTime dataFimLeitura)
    {
        // Regra de negócio: a nota deve ser de 1 a 5.
        if (nota < NotaMinima || nota > NotaMaxima)
            throw new RegraNegocioException(
                $"A nota deve ser de {NotaMinima} a {NotaMaxima}. Valor informado: {nota}.");

        // Regra de negócio: a data de início não pode ser maior que a data de fim de leitura.
        if (dataInicioLeitura.Date > dataFimLeitura.Date)
            throw new RegraNegocioException(
                $"A data de início da leitura ({dataInicioLeitura:dd/MM/yyyy}) não pode ser " +
                $"maior que a data de fim ({dataFimLeitura:dd/MM/yyyy}).");

        if (dataFimLeitura.Date > DateTime.Today)
            throw new RegraNegocioException("A data de fim da leitura não pode estar no futuro.");

        IdLivro = idLivro;
        IdUsuario = idUsuario;
        Nota = nota;
        Descricao = descricao?.Trim() ?? string.Empty;
        DataInicioLeitura = dataInicioLeitura.Date;
        DataFimLeitura = dataFimLeitura.Date;
        DataCriacao = DateTime.Now;
    }

    public int Nota { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public int IdUsuario { get; private set; }
    public int IdLivro { get; private set; }
    public DateTime DataInicioLeitura { get; private set; }
    public DateTime DataFimLeitura { get; private set; }

    public Livro? Livro { get; private set; }
    public Usuario? Usuario { get; private set; }

    public int DiasDeLeitura => (DataFimLeitura.Date - DataInicioLeitura.Date).Days + 1;
}
