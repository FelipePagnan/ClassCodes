using BancoSangue.Intermediario.Dominio.Entidades;

namespace BancoSangue.Intermediario.Aplicacao.ViewModels;

public class EstoqueViewModel
{
    public string TipoSanguineo { get; set; } = string.Empty;
    public int QuantidadeML { get; set; }
    public int QuantidadeMinimaML { get; set; }
    public bool EstoqueBaixo { get; set; }
    public string? Alerta { get; set; }

    public static EstoqueViewModel De(EstoqueSangue estoque) => new()
    {
        TipoSanguineo = estoque.Descricao,
        QuantidadeML = estoque.QuantidadeML,
        QuantidadeMinimaML = EstoqueSangue.QuantidadeMinimaML,
        EstoqueBaixo = estoque.EstoqueBaixo,
        Alerta = estoque.EstoqueBaixo
            ? $"Estoque do tipo {estoque.Descricao} abaixo do mínimo de {EstoqueSangue.QuantidadeMinimaML}ml."
            : null
    };
}

/// <summary>
/// Relatório da quantidade total de sangue disponível por tipo.
/// </summary>
public class RelatorioEstoqueViewModel
{
    public int TotalGeralML { get; set; }
    public int QuantidadeTiposComEstoqueBaixo { get; set; }
    public List<EstoqueViewModel> PorTipo { get; set; } = new();
}
