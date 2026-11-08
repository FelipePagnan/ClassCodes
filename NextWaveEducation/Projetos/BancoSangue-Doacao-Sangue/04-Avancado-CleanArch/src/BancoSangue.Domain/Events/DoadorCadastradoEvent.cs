using BancoSangue.Domain.Common;

namespace BancoSangue.Domain.Events;

public class DoadorCadastradoEvent : IDomainEvent
{
    public DoadorCadastradoEvent(int doadorId, string nomeCompleto, string email, bool aptoParaDoar)
    {
        DoadorId = doadorId;
        NomeCompleto = nomeCompleto;
        Email = email;
        AptoParaDoar = aptoParaDoar;
        OcorridoEm = DateTime.UtcNow;
    }

    public int DoadorId { get; }
    public string NomeCompleto { get; }
    public string Email { get; }
    public bool AptoParaDoar { get; }
    public DateTime OcorridoEm { get; }
}
