using BancoSangue.Domain.Common;

namespace BancoSangue.Domain.Events;

/// <summary>
/// Disparado quando o estoque de um tipo sanguíneo fica abaixo da quantidade mínima definida.
/// </summary>
public class EstoqueAbaixoDoMinimoEvent : IDomainEvent
{
    public EstoqueAbaixoDoMinimoEvent(string tipoSanguineo, int quantidadeAtualML, int quantidadeMinimaML)
    {
        TipoSanguineo = tipoSanguineo;
        QuantidadeAtualML = quantidadeAtualML;
        QuantidadeMinimaML = quantidadeMinimaML;
        OcorridoEm = DateTime.UtcNow;
    }

    public string TipoSanguineo { get; }
    public int QuantidadeAtualML { get; }
    public int QuantidadeMinimaML { get; }
    public DateTime OcorridoEm { get; }

    public string Mensagem =>
        $"Estoque do tipo {TipoSanguineo} está em {QuantidadeAtualML}ml, " +
        $"abaixo do mínimo de {QuantidadeMinimaML}ml.";
}
