using Livros.Domain.Common;

namespace Livros.Domain.Events;

public class LivroCadastradoEvent : IDomainEvent
{
    public LivroCadastradoEvent(int livroId, string titulo, string autor, string isbn)
    {
        LivroId = livroId;
        Titulo = titulo;
        Autor = autor;
        Isbn = isbn;
        OcorridoEm = DateTime.UtcNow;
    }

    public int LivroId { get; }
    public string Titulo { get; }
    public string Autor { get; }
    public string Isbn { get; }
    public DateTime OcorridoEm { get; }
}
