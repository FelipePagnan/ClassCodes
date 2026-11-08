using BancoSangue.Api.Enums;

namespace BancoSangue.Api.Entidades;

public class EstoqueSangue
{
    public const int QuantidadeMinimaAlerta = 2000;

    public int Id { get; set; }
    public TipoSanguineo TipoSanguineo { get; set; }
    public FatorRh FatorRh { get; set; }
    public int QuantidadeML { get; set; }

    public bool EstoqueBaixo => QuantidadeML < QuantidadeMinimaAlerta;
}
