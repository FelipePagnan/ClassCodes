using Clinica.Dominio.Entidades;

namespace Clinica.Dominio.Eventos;

/// <summary>
/// Marcador de evento de domínio: algo que aconteceu e que outras partes do
/// sistema podem querer saber.
///
/// A interface é vazia e mora no domínio de propósito. O domínio não conhece
/// MediatR nem qualquer biblioteca de mensageria — quem faz a ponte é a
/// camada de aplicação, que embala cada evento num INotification.
/// </summary>
public interface IEventoDeDominio
{
    DateTime OcorridoEm { get; }
}

/// <summary>Base com o carimbo de hora, para os eventos não repetirem isso.</summary>
public abstract record EventoDeDominio : IEventoDeDominio
{
    public DateTime OcorridoEm { get; } = DateTime.Now;
}

/// <summary>
/// Base dos eventos de atendimento.
///
/// O evento carrega a própria entidade, e não só o Id, por um motivo
/// concreto: num agendamento novo o Id só é atribuído no SaveChanges, e os
/// eventos são publicados depois disso. Guardando o Id no momento em que o
/// evento nasce, ele valeria zero. Guardando a entidade, o handler lê o Id
/// já preenchido — e ainda evita uma segunda ida ao banco.
/// </summary>
public abstract record EventoDeAtendimento(Atendimento Atendimento) : EventoDeDominio
{
    public int AtendimentoId => Atendimento.Id;
}

/// <summary>
/// Um atendimento foi agendado. Quem escuta: o envio da confirmação ao
/// paciente.
/// </summary>
public record AtendimentoAgendado(Atendimento Atendimento)
    : EventoDeAtendimento(Atendimento);

/// <summary>
/// O atendimento foi confirmado. Quem escuta: o e-mail com o .ics anexado e
/// o SMS com o link do Google Agenda.
/// </summary>
public record AtendimentoConfirmado(Atendimento Atendimento)
    : EventoDeAtendimento(Atendimento);

/// <summary>
/// O atendimento mudou de horário. Quem escuta: o reenvio da confirmação,
/// porque o convite que o paciente tem na agenda ficou errado.
/// </summary>
public record AtendimentoRemarcado(
    Atendimento Atendimento,
    DateTime DataAnterior) : EventoDeAtendimento(Atendimento);

/// <summary>
/// O atendimento foi cancelado. Quem escuta: o aviso ao paciente.
/// </summary>
public record AtendimentoCancelado(Atendimento Atendimento, string? Motivo)
    : EventoDeAtendimento(Atendimento);

/// <summary>O paciente não compareceu. Quem escuta: o relatório de faltas.</summary>
public record PacienteNaoCompareceu(Atendimento Atendimento)
    : EventoDeAtendimento(Atendimento);

/// <summary>
/// Um documento clínico foi anexado. Quem escuta: a trilha de auditoria — em
/// prontuário, quem anexou o quê e quando é informação que se guarda.
/// </summary>
public record DocumentoAnexado(Atendimento Atendimento, Anexo Anexo)
    : EventoDeAtendimento(Atendimento);

/// <summary>
/// O lembrete do dia anterior foi enviado. Quem escuta: a auditoria de
/// comunicação com o paciente.
/// </summary>
public record LembreteEnviado(Atendimento Atendimento)
    : EventoDeAtendimento(Atendimento);
