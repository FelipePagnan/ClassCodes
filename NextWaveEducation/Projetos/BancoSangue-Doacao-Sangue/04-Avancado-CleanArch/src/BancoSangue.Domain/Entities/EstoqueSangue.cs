using BancoSangue.Domain.Common;
using BancoSangue.Domain.Enums;
using BancoSangue.Domain.Events;
using BancoSangue.Domain.Exceptions;

namespace BancoSangue.Domain.Entities;

public class EstoqueSangue : BaseEntity
{
    /// <summary>Quantidade mínima em estoque por tipo sanguíneo, em mililitros.</summary>
    public const int QuantidadeMinimaML = 2000;

    private EstoqueSangue() { }

    public EstoqueSangue(TipoSanguineo tipoSanguineo, FatorRh fatorRh, int quantidadeML = 0)
    {
        if (quantidadeML < 0)
            throw new RegraNegocioException("A quantidade em estoque não pode ser negativa.");

        TipoSanguineo = tipoSanguineo;
        FatorRh = fatorRh;
        QuantidadeML = quantidadeML;
    }

    public TipoSanguineo TipoSanguineo { get; private set; }
    public FatorRh FatorRh { get; private set; }
    public int QuantidadeML { get; private set; }

    public string Descricao => $"{TipoSanguineo}{(FatorRh == FatorRh.Positivo ? "+" : "-")}";

    /// <summary>True quando o estoque está abaixo da quantidade mínima definida.</summary>
    public bool EstaAbaixoDoMinimo => QuantidadeML < QuantidadeMinimaML;

    /// <summary>
    /// Credita sangue no estoque. Se depois do crédito o estoque continuar abaixo
    /// do mínimo, dispara o evento de alerta.
    /// </summary>
    public void Adicionar(int quantidadeML)
    {
        if (quantidadeML <= 0)
            throw new RegraNegocioException("A quantidade adicionada deve ser maior que zero.");

        QuantidadeML += quantidadeML;
        VerificarEstoqueMinimo();
    }

    /// <summary>Debita sangue do estoque (saída para transfusão, descarte, validade).</summary>
    public void Remover(int quantidadeML)
    {
        if (quantidadeML <= 0)
            throw new RegraNegocioException("A quantidade removida deve ser maior que zero.");

        if (quantidadeML > QuantidadeML)
            throw new RegraNegocioException(
                $"Estoque insuficiente do tipo {Descricao}. Disponível: {QuantidadeML}ml, solicitado: {quantidadeML}ml.");

        QuantidadeML -= quantidadeML;
        VerificarEstoqueMinimo();
    }

    /// <summary>
    /// Dispara o evento de estoque baixo. Chamado também pelo HostedService
    /// que monitora periodicamente o estoque.
    /// </summary>
    public void VerificarEstoqueMinimo()
    {
        if (EstaAbaixoDoMinimo)
            AdicionarEvento(new EstoqueAbaixoDoMinimoEvent(Descricao, QuantidadeML, QuantidadeMinimaML));
    }
}
