namespace ECommerceModels.Enums;

/// <summary>
/// Tipo de evento registrado no histórico de atualizações de um pedido.
/// </summary>
public enum TipoAtualizacaoPedido
{
    Criacao = 0,
    PagamentoConfirmado = 1,
    Separacao = 2,
    Envio = 3,
    Entrega = 4,
    Cancelamento = 5,
    Observacao = 6
}
