using Caixinhas.Domain.Common;
using Caixinhas.Domain.Enums;

namespace Caixinhas.Domain.Events;

public class StatusObjetivoAlteradoEvent : IDomainEvent
{
    public StatusObjetivoAlteradoEvent(
        int objetivoId,
        string titulo,
        StatusObjetivo statusAnterior,
        StatusObjetivo statusAtual)
    {
        ObjetivoId = objetivoId;
        Titulo = titulo;
        StatusAnterior = statusAnterior;
        StatusAtual = statusAtual;
        OcorridoEm = DateTime.UtcNow;
    }

    public int ObjetivoId { get; }
    public string Titulo { get; }
    public StatusObjetivo StatusAnterior { get; }
    public StatusObjetivo StatusAtual { get; }
    public DateTime OcorridoEm { get; }
}
