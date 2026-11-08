using CRMDev.Domain.Enums;

namespace CRMDev.Domain.Entities;

public class Oportunidade : BaseEntity
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime? Prazo { get; set; }
    public decimal? Orcamento { get; set; }
    public string? Escopo { get; set; }
    public bool SuporteIncluso { get; set; }

    public StatusOportunidade Status { get; set; } = StatusOportunidade.EmAberto;
    public RazaoPerda? RazaoPerda { get; set; }

    public EtapaOportunidade Etapa { get; set; } = EtapaOportunidade.ContatoFeito;

    public Guid ContatoId { get; set; }
    public Contato Contato { get; set; } = null!;

    /// <summary>
    /// Marca a oportunidade como ganha e move para a etapa Fechado.
    /// </summary>
    public void MarcarGanho()
    {
        Status = StatusOportunidade.Ganho;
        Etapa = EtapaOportunidade.Fechado;
        RazaoPerda = null;
        AtualizadoEm = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca a oportunidade como perdida, move para a etapa Fechado e registra a razão.
    /// </summary>
    public void MarcarPerdido(RazaoPerda razao)
    {
        Status = StatusOportunidade.Perdido;
        Etapa = EtapaOportunidade.Fechado;
        RazaoPerda = razao;
        AtualizadoEm = DateTime.UtcNow;
    }

    /// <summary>
    /// Altera a etapa da oportunidade. Não permite alterar manualmente uma
    /// oportunidade já fechada (ganha ou perdida) — ela só sai de Fechado se
    /// for reaberta explicitamente via regra de negócio futura.
    /// </summary>
    public void AlterarEtapa(EtapaOportunidade novaEtapa)
    {
        if (Status != StatusOportunidade.EmAberto)
        {
            throw new InvalidOperationException("Não é possível alterar a etapa de uma oportunidade fechada (ganha ou perdida).");
        }

        Etapa = novaEtapa;
        AtualizadoEm = DateTime.UtcNow;
    }
}
