namespace Caixinhas.Intermediario.Dominio.Entidades;

/// <summary>
/// Entidade base com Id, data de criação e o marcador de soft delete.
/// </summary>
public abstract class EntidadeBase
{
    public int Id { get; set; }
    public DateTime DataCriacao { get; protected set; } = DateTime.Now;
    public bool EstaDeletado { get; protected set; }

    public void MarcarComoDeletado() => EstaDeletado = true;
    public void Restaurar() => EstaDeletado = false;
}
