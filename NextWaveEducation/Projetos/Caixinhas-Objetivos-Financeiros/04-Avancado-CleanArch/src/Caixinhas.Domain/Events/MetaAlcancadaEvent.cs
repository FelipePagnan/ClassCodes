using Caixinhas.Domain.Common;

namespace Caixinhas.Domain.Events;

/// <summary>
/// Disparado quando a caixinha atinge a meta. Num cenário real levaria a
/// uma notificação para o usuário.
/// </summary>
public class MetaAlcancadaEvent : IDomainEvent
{
    public MetaAlcancadaEvent(
        int objetivoId,
        string titulo,
        decimal quantidadeAlvo,
        decimal valorTotal,
        DateTime? prazo,
        int diasParaOPrazo)
    {
        ObjetivoId = objetivoId;
        Titulo = titulo;
        QuantidadeAlvo = quantidadeAlvo;
        ValorTotal = valorTotal;
        Prazo = prazo;
        DiasParaOPrazo = diasParaOPrazo;
        OcorridoEm = DateTime.UtcNow;
    }

    public int ObjetivoId { get; }
    public string Titulo { get; }
    public decimal QuantidadeAlvo { get; }
    public decimal ValorTotal { get; }
    public DateTime? Prazo { get; }

    /// <summary>Dias de folga em relação ao prazo. Negativo significa atraso.</summary>
    public int DiasParaOPrazo { get; }

    public DateTime OcorridoEm { get; }

    public decimal ValorExcedente => ValorTotal - QuantidadeAlvo;
}
