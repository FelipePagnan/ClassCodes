namespace ECommerceModels.Models;

/// <summary>
/// Item de um pedido: associação entre Pedido e Produto, com a
/// quantidade e o preço praticado no momento da compra.
/// Corresponde ao "PedidoItem" referenciado em Pedido.Items.
/// </summary>
public class PedidoProduto
{
    public int IdPedido { get; set; }
    public Pedido? Pedido { get; set; }

    public int IdProduto { get; set; }
    public Produto? Produto { get; set; }

    public int Quantidade { get; set; }

    public decimal Preco { get; set; }

    // 1 PedidoProduto -> N PedidoProdutoAvaliacao
    public ICollection<PedidoProdutoAvaliacao> Avaliacoes { get; set; } = new List<PedidoProdutoAvaliacao>();
}
