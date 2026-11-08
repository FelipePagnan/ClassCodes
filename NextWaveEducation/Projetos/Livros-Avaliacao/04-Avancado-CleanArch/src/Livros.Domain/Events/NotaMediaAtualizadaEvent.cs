using Livros.Domain.Common;

namespace Livros.Domain.Events;

/// <summary>
/// Disparado quando a nota média de um livro muda após o recálculo.
/// </summary>
public class NotaMediaAtualizadaEvent : IDomainEvent
{
    public NotaMediaAtualizadaEvent(
        int livroId,
        string tituloLivro,
        decimal notaMediaAnterior,
        decimal notaMediaAtual,
        int totalAvaliacoes)
    {
        LivroId = livroId;
        TituloLivro = tituloLivro;
        NotaMediaAnterior = notaMediaAnterior;
        NotaMediaAtual = notaMediaAtual;
        TotalAvaliacoes = totalAvaliacoes;
        OcorridoEm = DateTime.UtcNow;
    }

    public int LivroId { get; }
    public string TituloLivro { get; }
    public decimal NotaMediaAnterior { get; }
    public decimal NotaMediaAtual { get; }
    public int TotalAvaliacoes { get; }
    public DateTime OcorridoEm { get; }

    public decimal Variacao => NotaMediaAtual - NotaMediaAnterior;
}
