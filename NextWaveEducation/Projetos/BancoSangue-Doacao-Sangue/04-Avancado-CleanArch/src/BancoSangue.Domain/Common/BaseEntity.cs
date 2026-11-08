namespace BancoSangue.Domain.Common;

/// <summary>
/// Entidade base: concentra o Id e a coleção de eventos de domínio
/// disparados pela entidade, publicados pela Unit of Work ao salvar.
/// </summary>
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public int Id { get; protected set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AdicionarEvento(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void LimparEventos() => _domainEvents.Clear();
}
