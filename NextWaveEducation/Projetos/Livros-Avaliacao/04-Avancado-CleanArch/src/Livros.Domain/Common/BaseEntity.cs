namespace Livros.Domain.Common;

/// <summary>
/// Entidade base: concentra o Id, a data de criação e a coleção de eventos
/// de domínio disparados pela entidade, publicados pela Unit of Work.
/// </summary>
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public int Id { get; protected set; }
    public DateTime DataCriacao { get; protected set; } = DateTime.Now;

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AdicionarEvento(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void LimparEventos() => _domainEvents.Clear();
}
