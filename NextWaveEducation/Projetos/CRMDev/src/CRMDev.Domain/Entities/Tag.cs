namespace CRMDev.Domain.Entities;

public class Tag : BaseEntity
{
    public string Nome { get; set; } = string.Empty;

    public ICollection<ContatoTag> ContatoTags { get; set; } = new List<ContatoTag>();
}
