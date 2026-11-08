namespace CRMDev.Domain.Enums;

/// <summary>
/// V1: etapas fixas do funil de vendas (CRMDev V3 irá torná-las uma entidade configurável).
/// </summary>
public enum EtapaOportunidade
{
    ContatoFeito = 1,
    PreQualificacao = 2,
    Conexao = 3,
    ReuniaoAgendada = 4,
    PropostaEnviada = 5,
    AguardandoAssinatura = 6,
    Fechado = 7
}
