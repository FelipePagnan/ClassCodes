using ECommerceModels.Enums;

namespace ECommerceModels.Models;

/// <summary>
/// Pedido realizado por um cliente.
/// </summary>
public class Pedido
{
    public int Id { get; set; }

    public int IdCliente { get; set; }
    public Cliente? Cliente { get; set; }

    // Itens do pedido (no diagrama: Items: PedidoItem[])
    public ICollection<PedidoProduto> Items { get; set; } = new List<PedidoProduto>();

    public DateTime? DataDeConfirmacao { get; set; }

    public DateTime? DataDeEnvio { get; set; }

    public StatusPedido Status { get; set; }

    public int EnderecoDeEntregaId { get; set; }
    public Endereco? EnderecoDeEntrega { get; set; }

    public decimal ValorFrete { get; set; }

    public decimal ValorTotal { get; set; }

    // 1 Pedido -> 1 PedidoPagamento
    public PedidoPagamento? Pagamento { get; set; }

    // 1 Pedido -> N PedidoAtualizacao
    public ICollection<PedidoAtualizacao> Atualizacoes { get; set; } = new List<PedidoAtualizacao>();
}
