namespace Livros.Intermediario.Dominio.Entidades;

public abstract class EntidadeBase
{
    public int Id { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.Now;
}
