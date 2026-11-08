namespace ECommerceModels.Enums;

/// <summary>
/// Status de um pagamento de pedido junto ao gateway externo.
/// </summary>
public enum StatusPagamento
{
    Pendente = 0,
    Processando = 1,
    Aprovado = 2,
    Recusado = 3,
    Estornado = 4,
    Cancelado = 5
}
