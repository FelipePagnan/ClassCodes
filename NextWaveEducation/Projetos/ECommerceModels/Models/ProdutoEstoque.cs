namespace ECommerceModels.Models;

/// <summary>
/// Representa uma posição/variação de estoque de um produto
/// (ex.: estoque por depósito, tamanho ou cor).
/// Referenciada no diagrama como Produto.Estoque: ProdutoEstoque[].
/// </summary>
public class ProdutoEstoque
{
    public int Id { get; set; }

    public int Quantidade { get; set; }

    // Relacionamento: 1 Produto -> N ProdutoEstoque
    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }
}
