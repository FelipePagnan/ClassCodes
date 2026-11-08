using Livros.Domain.Common;

namespace Livros.Domain.Events;

public class UsuarioCadastradoEvent : IDomainEvent
{
    public UsuarioCadastradoEvent(int usuarioId, string nome, string email)
    {
        UsuarioId = usuarioId;
        Nome = nome;
        Email = email;
        OcorridoEm = DateTime.UtcNow;
    }

    public int UsuarioId { get; }
    public string Nome { get; }
    public string Email { get; }
    public DateTime OcorridoEm { get; }
}
