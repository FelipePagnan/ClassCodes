using ECommerceModels.Enums;

namespace ECommerceModels.Models;

/// <summary>
/// Produto do catálogo da loja.
/// </summary>
public class Produto
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public decimal Preco { get; set; }

    public Categoria Categoria { get; set; }

    public string Marca { get; set; } = string.Empty;

    // 1 Produto -> N ProdutoEstoque
    public ICollection<ProdutoEstoque> Estoque { get; set; } = new List<ProdutoEstoque>();

    // Quantidade total consolidada (campo privado no diagrama).
    private int Quantidade { get; set; }

    /// <summary>
    /// Quantidade total em estoque, somando todas as posições de ProdutoEstoque.
    /// </summary>
    public int ObterQuantidadeTotal()
    {
        Quantidade = Estoque.Sum(e => e.Quantidade);
        return Quantidade;
    }

    // 1 Produto -> N ProdutoImagem
    public ICollection<ProdutoImagem> Imagens { get; set; } = new List<ProdutoImagem>();
}
