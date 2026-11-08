namespace ECommerceModels.Models;

/// <summary>
/// Endereço utilizado por um cliente (cadastro) ou como endereço de entrega de um pedido.
/// </summary>
public class Endereco
{
    public int Id { get; set; }

    public string Logradouro { get; set; } = string.Empty;

    public string Numero { get; set; } = string.Empty;

    public string? Complemento { get; set; }

    public string CEP { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string Cidade { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public string Pais { get; set; } = string.Empty;

    // Relacionamento: 1 Cliente -> N Endereços
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
}
