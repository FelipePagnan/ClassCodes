namespace ECommerceModels.Models;

/// <summary>
/// Avaliação feita por um cliente sobre um item (produto) de um pedido já recebido.
/// </summary>
public class PedidoProdutoAvaliacao
{
    public int Id { get; set; }

    public int IdPedidoProduto { get; set; }
    public PedidoProduto? PedidoProduto { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public int Nota { get; set; }

    public int IdCliente { get; set; }
    public Cliente? Cliente { get; set; }
}
