namespace CRMDev.Domain.Entities;

/// <summary>
/// Área de atuação do contato. Já nasce como entidade (cadastro), conforme
/// indicado no documento de escopo, para permitir cadastro livre pelo usuário.
/// </summary>
public class AreaAtuacao : BaseEntity
{
    public string Nome { get; set; } = string.Empty;

    public ICollection<Contato> Contatos { get; set; } = new List<Contato>();
}
