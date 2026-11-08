using Clinica.Aplicacao.Comum;
using Clinica.Aplicacao.Notificacoes;
using Clinica.Dominio.Eventos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Clinica.Aplicacao.EventHandlers;

/// <summary>
/// Quando o atendimento é confirmado, o paciente recebe o e-mail com o
/// convite .ics e o SMS com o link do Google Agenda.
///
/// Isto estar num handler de evento, e não dentro do comando Confirmar, é o
/// ponto do padrão: confirmar um atendimento é uma coisa; avisar o paciente
/// é outra. O comando não precisa saber que existe notificação, e amanhã
/// alguém pode acrescentar um aviso no WhatsApp sem tocar no comando.
/// </summary>
public class EnviarConfirmacaoAoConfirmar
    : INotificationHandler<NotificacaoDeEvento<AtendimentoConfirmado>>
{
    private readonly MontadorDeMensagens _montador;
    private readonly ILogger<EnviarConfirmacaoAoConfirmar> _logger;

    public EnviarConfirmacaoAoConfirmar(
        MontadorDeMensagens montador, ILogger<EnviarConfirmacaoAoConfirmar> logger)
    {
        _montador = montador;
        _logger = logger;
    }

    public Task Handle(
        NotificacaoDeEvento<AtendimentoConfirmado> notificacao,
        CancellationToken cancellationToken)
    {
        var atendimento = notificacao.Evento.Atendimento;
        var enviadas = _montador.Enviar(atendimento, lembrete: false);

        _logger.LogInformation(
            "Atendimento {AtendimentoId} confirmado: {Quantidade} notificação(ões) enviada(s).",
            atendimento.Id, enviadas.Count);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Remarcou, o convite que o paciente já tem na agenda está errado: a
/// confirmação é reenviada com o horário novo.
/// </summary>
public class ReenviarConfirmacaoAoRemarcar
    : INotificationHandler<NotificacaoDeEvento<AtendimentoRemarcado>>
{
    private readonly MontadorDeMensagens _montador;
    private readonly ILogger<ReenviarConfirmacaoAoRemarcar> _logger;

    public ReenviarConfirmacaoAoRemarcar(
        MontadorDeMensagens montador, ILogger<ReenviarConfirmacaoAoRemarcar> logger)
    {
        _montador = montador;
        _logger = logger;
    }

    public Task Handle(
        NotificacaoDeEvento<AtendimentoRemarcado> notificacao,
        CancellationToken cancellationToken)
    {
        var evento = notificacao.Evento;

        _montador.Enviar(evento.Atendimento, lembrete: false);

        _logger.LogInformation(
            "Atendimento {AtendimentoId} remarcado de {Anterior:dd/MM HH:mm} "
            + "para {Nova:dd/MM HH:mm}; paciente avisado.",
            evento.AtendimentoId, evento.DataAnterior, evento.Atendimento.DataHora);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Cancelou: o paciente é avisado por SMS. Não vai .ics aqui — o convite de
/// cancelamento seria mais confuso do que útil.
/// </summary>
public class AvisarCancelamento
    : INotificationHandler<NotificacaoDeEvento<AtendimentoCancelado>>
{
    private readonly Contratos.IEnviadorDeNotificacao _enviador;
    private readonly ILogger<AvisarCancelamento> _logger;

    public AvisarCancelamento(
        Contratos.IEnviadorDeNotificacao enviador, ILogger<AvisarCancelamento> logger)
    {
        _enviador = enviador;
        _logger = logger;
    }

    public Task Handle(
        NotificacaoDeEvento<AtendimentoCancelado> notificacao,
        CancellationToken cancellationToken)
    {
        var evento = notificacao.Evento;
        var atendimento = evento.Atendimento;

        var texto = $"Clinica Vida: seu atendimento de "
                    + $"{atendimento.DataHora:dd/MM} as {atendimento.DataHora:HH:mm} "
                    + "foi cancelado."
                    + (string.IsNullOrWhiteSpace(evento.Motivo)
                        ? " Ligue para a recepcao para remarcar."
                        : $" Motivo: {evento.Motivo}");

        _enviador.Enviar(
            atendimento.Id,
            Contratos.CanalNotificacao.Sms,
            atendimento.Paciente.Celular.Formatado,
            "Atendimento cancelado",
            texto);

        _logger.LogInformation(
            "Atendimento {AtendimentoId} cancelado; paciente avisado.", atendimento.Id);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Trilha de auditoria dos documentos clínicos. Em prontuário, quem anexou o
/// quê e quando é informação que se guarda — aqui vai para o log; num
/// sistema de verdade iria para uma tabela de auditoria.
/// </summary>
public class RegistrarDocumentoNaAuditoria
    : INotificationHandler<NotificacaoDeEvento<DocumentoAnexado>>
{
    private readonly ILogger<RegistrarDocumentoNaAuditoria> _logger;

    public RegistrarDocumentoNaAuditoria(ILogger<RegistrarDocumentoNaAuditoria> logger)
        => _logger = logger;

    public Task Handle(
        NotificacaoDeEvento<DocumentoAnexado> notificacao,
        CancellationToken cancellationToken)
    {
        var evento = notificacao.Evento;

        _logger.LogInformation(
            "AUDITORIA: {Tipo} '{Arquivo}' ({Tamanho}) anexado ao atendimento "
            + "{AtendimentoId} do paciente {Paciente} em {Quando:dd/MM/yyyy HH:mm}.",
            evento.Anexo.Tipo,
            evento.Anexo.NomeArquivo,
            evento.Anexo.TamanhoLegivel,
            evento.AtendimentoId,
            evento.Atendimento.Paciente.NomeCompleto,
            evento.OcorridoEm);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Registro das faltas. Alimenta o indicador de não comparecimento, que é o
/// número que uma clínica usa para decidir se vale cobrar taxa de falta.
/// </summary>
public class RegistrarFalta
    : INotificationHandler<NotificacaoDeEvento<PacienteNaoCompareceu>>
{
    private readonly ILogger<RegistrarFalta> _logger;

    public RegistrarFalta(ILogger<RegistrarFalta> logger) => _logger = logger;

    public Task Handle(
        NotificacaoDeEvento<PacienteNaoCompareceu> notificacao,
        CancellationToken cancellationToken)
    {
        var atendimento = notificacao.Evento.Atendimento;

        _logger.LogInformation(
            "FALTA: {Paciente} não compareceu ao atendimento {AtendimentoId} "
            + "de {DataHora:dd/MM/yyyy HH:mm} com {Medico}.",
            atendimento.Paciente.NomeCompleto,
            atendimento.Id,
            atendimento.DataHora,
            atendimento.Medico.NomeCompleto);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Agendado (ainda não confirmado) é só anotado. A confirmação sai quando a
/// recepção confirma, não no momento da marcação — e é por isso que agendar
/// e confirmar são dois eventos diferentes.
/// </summary>
public class RegistrarAgendamento
    : INotificationHandler<NotificacaoDeEvento<AtendimentoAgendado>>
{
    private readonly ILogger<RegistrarAgendamento> _logger;

    public RegistrarAgendamento(ILogger<RegistrarAgendamento> logger) => _logger = logger;

    public Task Handle(
        NotificacaoDeEvento<AtendimentoAgendado> notificacao,
        CancellationToken cancellationToken)
    {
        var atendimento = notificacao.Evento.Atendimento;

        _logger.LogInformation(
            "Atendimento {AtendimentoId} agendado: {Paciente} com {Medico} "
            + "em {DataHora:dd/MM/yyyy HH:mm} ({Servico}).",
            atendimento.Id,
            atendimento.Paciente.NomeCompleto,
            atendimento.Medico.NomeCompleto,
            atendimento.DataHora,
            atendimento.Servico.Nome);

        return Task.CompletedTask;
    }
}

/// <summary>Auditoria do envio de lembrete.</summary>
public class RegistrarLembreteNaAuditoria
    : INotificationHandler<NotificacaoDeEvento<LembreteEnviado>>
{
    private readonly ILogger<RegistrarLembreteNaAuditoria> _logger;

    public RegistrarLembreteNaAuditoria(ILogger<RegistrarLembreteNaAuditoria> logger)
        => _logger = logger;

    public Task Handle(
        NotificacaoDeEvento<LembreteEnviado> notificacao,
        CancellationToken cancellationToken)
    {
        var atendimento = notificacao.Evento.Atendimento;

        _logger.LogInformation(
            "AUDITORIA: lembrete do atendimento {AtendimentoId} enviado a {Paciente}.",
            atendimento.Id, atendimento.Paciente.NomeCompleto);

        return Task.CompletedTask;
    }
}
