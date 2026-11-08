namespace ECommerceModels.Enums;

/// <summary>
/// Status do ciclo de vida de um pedido.
/// </summary>
public enum StatusPedido
{
    AguardandoPagamento = 0,
    PagamentoConfirmado = 1,
    EmSeparacao = 2,
    Enviado = 3,
    Entregue = 4,
    Cancelado = 5,
    Devolvido = 6
}
