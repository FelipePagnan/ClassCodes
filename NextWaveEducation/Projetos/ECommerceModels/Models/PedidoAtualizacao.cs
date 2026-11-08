using ECommerceModels.Enums;

namespace ECommerceModels.Models;

/// <summary>
/// Entrada de histórico/timeline de um pedido (ex.: mudanças de status).
/// </summary>
public class PedidoAtualizacao
{
    public int Id { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public TipoAtualizacaoPedido Tipo { get; set; }

    public int IdPedido { get; set; }
    public Pedido? Pedido { get; set; }
}
