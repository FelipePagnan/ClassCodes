namespace ECommerceModels.Models;

/// <summary>
/// Cliente da loja. Possui relação 1 (Cliente) para N (Endereços).
/// </summary>
public class Cliente
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Sobrenome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;

    public DateTime DataNascimento { get; set; }

    public string Documento { get; set; } = string.Empty;

    // 1 Cliente -> N Endereços
    public ICollection<Endereco> Enderecos { get; set; } = new List<Endereco>();
}
