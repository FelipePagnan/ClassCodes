using BancoSangue.Intermediario.Dominio.Enums;

namespace BancoSangue.Intermediario.Dominio.Entidades;

public class EstoqueSangue : EntidadeBase
{
    /// <summary>
    /// Quantidade mínima (em ml) por tipo sanguíneo. Abaixo disso o sistema emite alerta.
    /// </summary>
    public const int QuantidadeMinimaML = 2000;

    protected EstoqueSangue() { }

    public EstoqueSangue(TipoSanguineo tipoSanguineo, FatorRh fatorRh, int quantidadeML = 0)
    {
        TipoSanguineo = tipoSanguineo;
        FatorRh = fatorRh;
        QuantidadeML = quantidadeML;
    }

    public TipoSanguineo TipoSanguineo { get; private set; }
    public FatorRh FatorRh { get; private set; }
    public int QuantidadeML { get; private set; }

    public string Descricao => $"{TipoSanguineo}{(FatorRh == FatorRh.Positivo ? "+" : "-")}";

    public bool EstoqueBaixo => QuantidadeML < QuantidadeMinimaML;

    public void Adicionar(int quantidadeML) => QuantidadeML += quantidadeML;

    public void Remover(int quantidadeML)
        => QuantidadeML = Math.Max(0, QuantidadeML - quantidadeML);
}
