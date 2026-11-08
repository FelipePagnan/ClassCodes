using Caixinhas.Domain.Common;
using Caixinhas.Domain.Enums;

namespace Caixinhas.Domain.Events;

/// <summary>
/// Disparado quando uma transação é registrada. O handler na camada de
/// Application confere o ValorTotal e registra a movimentação em log.
/// </summary>
public class TransacaoRegistradaEvent : IDomainEvent
{
    public TransacaoRegistradaEvent(
        int objetivoId,
        string tituloObjetivo,
        decimal quantidade,
        TipoTransacao tipo,
        DateTime dataTransacao,
        decimal valorTotalAnterior,
        decimal valorTotalAtual)
    {
        ObjetivoId = objetivoId;
        TituloObjetivo = tituloObjetivo;
        Quantidade = quantidade;
        Tipo = tipo;
        DataTransacao = dataTransacao;
        ValorTotalAnterior = valorTotalAnterior;
        ValorTotalAtual = valorTotalAtual;
        OcorridoEm = DateTime.UtcNow;
    }

    public int ObjetivoId { get; }
    public string TituloObjetivo { get; }
    public decimal Quantidade { get; }
    public TipoTransacao Tipo { get; }
    public DateTime DataTransacao { get; }

    /// <summary>Saldo antes da transação.</summary>
    public decimal ValorTotalAnterior { get; }

    /// <summary>Saldo depois da transação, já atualizado pela entidade.</summary>
    public decimal ValorTotalAtual { get; }

    public DateTime OcorridoEm { get; }
}
