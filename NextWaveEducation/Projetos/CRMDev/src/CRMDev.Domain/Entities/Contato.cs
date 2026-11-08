namespace CRMDev.Domain.Entities;

public class Contato : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? TelefoneCelular { get; set; }
    public string? Cargo { get; set; }
    public string? Documento { get; set; }
    public string? Endereco { get; set; }
    public string? Notas { get; set; }

    public Guid? AreaAtuacaoId { get; set; }
    public AreaAtuacao? AreaAtuacao { get; set; }

    public ICollection<ContatoTag> ContatoTags { get; set; } = new List<ContatoTag>();
    public ICollection<Oportunidade> Oportunidades { get; set; } = new List<Oportunidade>();
}
