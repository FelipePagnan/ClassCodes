namespace CRMDev.Domain.Entities;

/// <summary>
/// Entidade de associação N:N entre Contato e Tag.
/// </summary>
public class ContatoTag
{
    public Guid ContatoId { get; set; }
    public Contato Contato { get; set; } = null!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
