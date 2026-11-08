using Livros.Domain.Common;

namespace Livros.Domain.Events;

/// <summary>
/// Disparado quando uma avaliação é cadastrada. O handler na camada de
/// Application recalcula e persiste a nota média do livro, desacoplando
/// as duas operações.
/// </summary>
public class AvaliacaoCadastradaEvent : IDomainEvent
{
    public AvaliacaoCadastradaEvent(
        int livroId,
        string tituloLivro,
        int usuarioId,
        int nota,
        DateTime fimDaLeitura)
    {
        LivroId = livroId;
        TituloLivro = tituloLivro;
        UsuarioId = usuarioId;
        Nota = nota;
        FimDaLeitura = fimDaLeitura;
        OcorridoEm = DateTime.UtcNow;
    }

    public int LivroId { get; }
    public string TituloLivro { get; }
    public int UsuarioId { get; }
    public int Nota { get; }
    public DateTime FimDaLeitura { get; }
    public DateTime OcorridoEm { get; }
}
