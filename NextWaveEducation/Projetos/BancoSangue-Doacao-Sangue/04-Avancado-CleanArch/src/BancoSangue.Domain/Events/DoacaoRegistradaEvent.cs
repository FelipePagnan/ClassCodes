using BancoSangue.Domain.Common;
using BancoSangue.Domain.Enums;

namespace BancoSangue.Domain.Events;

/// <summary>
/// Disparado quando uma doação é registrada. O handler na camada de Application
/// é quem atualiza o estoque de sangue, desacoplando as duas operações.
/// </summary>
public class DoacaoRegistradaEvent : IDomainEvent
{
    public DoacaoRegistradaEvent(
        int doadorId,
        string nomeDoador,
        TipoSanguineo tipoSanguineo,
        FatorRh fatorRh,
        int quantidadeML,
        DateTime dataDoacao)
    {
        DoadorId = doadorId;
        NomeDoador = nomeDoador;
        TipoSanguineo = tipoSanguineo;
        FatorRh = fatorRh;
        QuantidadeML = quantidadeML;
        DataDoacao = dataDoacao;
        OcorridoEm = DateTime.UtcNow;
    }

    public int DoadorId { get; }
    public string NomeDoador { get; }
    public TipoSanguineo TipoSanguineo { get; }
    public FatorRh FatorRh { get; }
    public int QuantidadeML { get; }
    public DateTime DataDoacao { get; }
    public DateTime OcorridoEm { get; }
}
