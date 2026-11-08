using Clinica.Console.Enums;
using Clinica.Console.Excecoes;

namespace Clinica.Console.Entidades;

/// <summary>
/// O agendamento de um serviço, para um paciente, com um médico, num horário.
/// É o centro do sistema: tudo mais existe para alimentá-lo.
/// </summary>
public class Atendimento
{
    public int Id { get; set; }

    public int PacienteId { get; set; }
    public Paciente? Paciente { get; set; }

    public int MedicoId { get; set; }
    public Medico? Medico { get; set; }

    public int ServicoId { get; set; }
    public Servico? Servico { get; set; }

    public DateTime DataHora { get; set; }

    /// <summary>
    /// Duração copiada do serviço no momento do agendamento. Guardar aqui
    /// evita que mudar a duração do serviço mexa em agendas já marcadas.
    /// </summary>
    public int DuracaoMinutos { get; set; }

    /// <summary>Valor cobrado, também copiado do serviço no agendamento.</summary>
    public decimal Valor { get; set; }

    public StatusAtendimento Status { get; set; } = StatusAtendimento.Agendado;

    public string? Observacoes { get; set; }

    /// <summary>Quando a confirmação foi enviada ao paciente.</summary>
    public DateTime? ConfirmacaoEnviadaEm { get; set; }

    /// <summary>Quando o lembrete do dia anterior foi enviado.</summary>
    public DateTime? LembreteEnviadoEm { get; set; }

    public List<Anexo> Anexos { get; set; } = new();

    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public bool EstaDeletado { get; set; }

    public DateTime DataHoraFim => DataHora.AddMinutes(DuracaoMinutos);

    public IEnumerable<Anexo> AnexosAtivos => Anexos.Where(a => !a.EstaDeletado);

    /// <summary>Atendimento que ainda vai acontecer e não foi cancelado.</summary>
    public bool EstaAberto
        => Status is StatusAtendimento.Agendado or StatusAtendimento.Confirmado;

    /// <summary>Status finais: o atendimento não muda mais.</summary>
    public bool EstaEncerrado
        => Status is StatusAtendimento.Realizado
                  or StatusAtendimento.Cancelado
                  or StatusAtendimento.NaoCompareceu;

    public bool EhHoje => DataHora.Date == DateTime.Today;

    public bool EhAmanha => DataHora.Date == DateTime.Today.AddDays(1);

    public bool JaPassou => DataHora < DateTime.Now;

    /// <summary>
    /// Verifica se este atendimento se sobrepõe no tempo com outro.
    /// Dois intervalos colidem quando cada um começa antes do outro terminar.
    /// </summary>
    public bool ConflitaCom(Atendimento outro)
        => DataHora < outro.DataHoraFim && outro.DataHora < DataHoraFim;

    // ----------------------------- Comportamento -----------------------------

    public void Confirmar()
    {
        if (Status != StatusAtendimento.Agendado)
            throw new RegraNegocioException(
                $"Só um atendimento agendado pode ser confirmado. Status atual: {Status}.");

        Status = StatusAtendimento.Confirmado;
        ConfirmacaoEnviadaEm = DateTime.Now;
    }

    public void MarcarComoRealizado()
    {
        if (!EstaAberto)
            throw new RegraNegocioException(
                $"Um atendimento {Status} não pode ser marcado como realizado.");

        if (DataHora > DateTime.Now)
            throw new RegraNegocioException(
                $"O atendimento está marcado para {DataHora:dd/MM/yyyy HH:mm} e ainda não aconteceu.");

        Status = StatusAtendimento.Realizado;
    }

    public void Cancelar(string? motivo = null)
    {
        if (Status == StatusAtendimento.Realizado)
            throw new RegraNegocioException("Um atendimento realizado não pode ser cancelado.");

        if (Status == StatusAtendimento.Cancelado)
            throw new RegraNegocioException("Este atendimento já está cancelado.");

        Status = StatusAtendimento.Cancelado;

        if (!string.IsNullOrWhiteSpace(motivo))
        {
            Observacoes = string.IsNullOrWhiteSpace(Observacoes)
                ? $"Cancelado: {motivo}"
                : $"{Observacoes} | Cancelado: {motivo}";
        }
    }

    public void MarcarComoNaoCompareceu()
    {
        if (!EstaAberto)
            throw new RegraNegocioException(
                $"Um atendimento {Status} não pode ser marcado como falta.");

        if (DataHora > DateTime.Now)
            throw new RegraNegocioException(
                "Não é possível marcar falta num atendimento que ainda não aconteceu.");

        Status = StatusAtendimento.NaoCompareceu;
    }

    public void RegistrarLembreteEnviado() => LembreteEnviadoEm = DateTime.Now;

    /// <summary>
    /// Anexos só entram em atendimentos realizados: atestado, receita e
    /// evolução são produzidos durante a consulta.
    /// </summary>
    public void GarantirQueAceitaAnexos()
    {
        if (Status != StatusAtendimento.Realizado)
            throw new RegraNegocioException(
                $"Anexos só podem ser adicionados a atendimentos realizados. " +
                $"Status atual: {Status}.");
    }

    public override string ToString()
        => $"[{Id}] {DataHora:dd/MM/yyyy HH:mm} ({DuracaoMinutos}min)  " +
           $"{Paciente?.NomeCompleto,-24} {Medico?.NomeCompleto,-22} " +
           $"{Servico?.Nome,-22} {Status}";
}
