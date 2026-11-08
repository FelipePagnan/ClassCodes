using Caixinhas.Domain.Common;

namespace Caixinhas.Domain.Events;

public class ObjetivoCriadoEvent : IDomainEvent
{
    public ObjetivoCriadoEvent(int objetivoId, string titulo, decimal quantidadeAlvo, DateTime? prazo)
    {
        ObjetivoId = objetivoId;
        Titulo = titulo;
        QuantidadeAlvo = quantidadeAlvo;
        Prazo = prazo;
        OcorridoEm = DateTime.UtcNow;
    }

    public int ObjetivoId { get; }
    public string Titulo { get; }
    public decimal QuantidadeAlvo { get; }
    public DateTime? Prazo { get; }
    public DateTime OcorridoEm { get; }
}
