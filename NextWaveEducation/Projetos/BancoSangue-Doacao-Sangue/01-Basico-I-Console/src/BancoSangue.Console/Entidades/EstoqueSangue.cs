using BancoSangue.Console.Enums;

namespace BancoSangue.Console.Entidades;

public class EstoqueSangue
{
    public int Id { get; set; }
    public TipoSanguineo TipoSanguineo { get; set; }
    public FatorRh FatorRh { get; set; }
    public int QuantidadeML { get; set; }

    public const int QuantidadeMinimaAlerta = 2000; // 2 litros por tipo, usado para o alerta de estoque baixo

    public string Descricao => $"{TipoSanguineo}{(FatorRh == FatorRh.Positivo ? "+" : "-")}";

    public bool EstoqueBaixo => QuantidadeML < QuantidadeMinimaAlerta;

    public override string ToString()
        => $"{Descricao}: {QuantidadeML}ml{(EstoqueBaixo ? "  [ESTOQUE BAIXO]" : string.Empty)}";
}
