using ECommerceModels.Enums;

namespace ECommerceModels.Models;

/// <summary>
/// Dados de pagamento de um pedido junto a um gateway de pagamento externo.
/// </summary>
public class PedidoPagamento
{
    public int Id { get; set; }

    public string IdPagamentoExterno { get; set; } = string.Empty;

    public StatusPagamento Status { get; set; }

    public DateTime? DataDeConfirmacao { get; set; }

    public int IdPedido { get; set; }
    public Pedido? Pedido { get; set; }
}
