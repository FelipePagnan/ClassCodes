namespace Caixinhas.Domain.Common;

/// <summary>
/// Entidade base: Id, data de criação, marcador de soft delete e a coleção
/// de eventos de domínio publicados pela Unit of Work ao salvar.
/// </summary>
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public int Id { get; protected set; }
    public DateTime DataCriacao { get; protected set; } = DateTime.Now;

    /// <summary>Soft delete: o registro sai das consultas mas continua no banco.</summary>
    public bool EstaDeletado { get; protected set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AdicionarEvento(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void LimparEventos() => _domainEvents.Clear();

    protected void MarcarComoDeletado() => EstaDeletado = true;
}
