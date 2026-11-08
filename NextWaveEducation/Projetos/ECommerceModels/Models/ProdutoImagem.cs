namespace ECommerceModels.Models;

/// <summary>
/// Imagem associada a um produto (0..N por produto).
/// </summary>
public class ProdutoImagem
{
    public string Identificador { get; set; } = string.Empty;

    public string Rota { get; set; } = string.Empty;

    public bool EstaVisivel { get; set; }

    // Relacionamento: 1 Produto -> N ProdutoImagem
    public int ProdutoId { get; set; }
    public Produto? Produto { get; set; }
}
