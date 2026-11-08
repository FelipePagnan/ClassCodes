using System.Text;
using Clinica.Aplicacao.Contratos;
using Clinica.Dominio.Entidades;
using Clinica.Dominio.Excecoes;
using Clinica.Dominio.Repositorios;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Clinica.Aplicacao.Notificacoes;

/// <summary>Resultado de uma rodada de lembretes.</summary>
public record RodadaLembretes(
    DateTime ExecutadaEm,
    int AtendimentosEncontrados,
    List<Notificacao> Enviadas);

// ============================== Comandos e consultas ==============================

/// <summary>
/// Dispara a rodada de lembretes. É um comando porque muda estado: cada
/// atendimento notificado recebe o carimbo LembreteEnviadoEm.
///
/// Quem chama é o HostedService — e também o endpoint manual, útil para
/// testar sem esperar o próximo ciclo.
/// </summary>
public record ExecutarRodadaDeLembretes : IRequest<RodadaLembretes>;

public record ListarNotificacoes(int? AtendimentoId) : IRequest<List<Notificacao>>;

public record ObterNotificacao(int Id) : IRequest<Notificacao?>;

public record ObterConviteIcs(int AtendimentoId) : IRequest<string>;

public record ObterLinkGoogleAgenda(int AtendimentoId) : IRequest<string>;

/// <summary>Reenvia a confirmação sem mexer no status do atendimento.</summary>
public record ReenviarConfirmacao(int AtendimentoId) : IRequest<List<Notificacao>>;

// ============================= Montagem das mensagens =============================

/// <summary>
/// Monta o texto das mensagens. Separado dos handlers porque confirmação,
/// reenvio e lembrete usam o mesmo conteúdo com pequenas variações — e
/// porque texto de mensagem é a parte que mais muda a pedido de quem atende
/// no balcão.
/// </summary>
public class MontadorDeMensagens
{
    private readonly IEnviadorDeNotificacao _enviador;
    private readonly IGeradorDeConviteDeCalendario _calendario;

    public MontadorDeMensagens(
        IEnviadorDeNotificacao enviador, IGeradorDeConviteDeCalendario calendario)
    {
        _enviador = enviador;
        _calendario = calendario;
    }

    /// <summary>
    /// Envia nos dois canais: e-mail com o .ics anexado (quando há e-mail) e
    /// SMS com o link do Google Agenda.
    /// </summary>
    public List<Notificacao> Enviar(Atendimento atendimento, bool lembrete)
    {
        var paciente = atendimento.Paciente;
        var link = _calendario.GerarLinkGoogleAgenda(atendimento);
        var ics = _calendario.GerarIcs(atendimento);
        var enviadas = new List<Notificacao>();

        if (paciente.Email is not null)
        {
            var assunto = lembrete
                ? $"Lembrete: seu atendimento é amanhã às {atendimento.DataHora:HH:mm}"
                : $"Confirmação do seu atendimento em {atendimento.DataHora:dd/MM}";

            enviadas.Add(_enviador.Enviar(
                atendimento.Id, CanalNotificacao.Email, paciente.Email.Valor,
                assunto, MontarEmail(atendimento, link, lembrete), link, ics));
        }

        enviadas.Add(_enviador.Enviar(
            atendimento.Id, CanalNotificacao.Sms, paciente.Celular.Formatado,
            lembrete ? "Lembrete de atendimento" : "Confirmação de atendimento",
            MontarSms(atendimento, link, lembrete), link));

        return enviadas;
    }

    private static string MontarEmail(Atendimento atendimento, string link, bool lembrete)
    {
        var corpo = new StringBuilder();
        corpo.AppendLine($"Olá, {atendimento.Paciente.PrimeiroNome}!");
        corpo.AppendLine();

        corpo.AppendLine(lembrete
            ? "Passando para lembrar do seu atendimento de amanhã, "
              + $"{atendimento.DataHora:dd/MM/yyyy}, às {atendimento.DataHora:HH:mm}."
            : "Seu atendimento está confirmado:");

        corpo.AppendLine();
        corpo.AppendLine($"  Serviço: {atendimento.Servico.Nome}");
        corpo.AppendLine($"  Profissional: {atendimento.Medico.NomeCompleto} "
                         + $"({atendimento.Medico.Crm.Completo})");
        corpo.AppendLine($"  Data: {atendimento.DataHora:dd/MM/yyyy} "
                         + $"({DiaDaSemana(atendimento.DataHora)})");
        corpo.AppendLine($"  Horário: {atendimento.DataHora:HH:mm} às "
                         + $"{atendimento.DataHoraFim:HH:mm}");

        if (!lembrete) corpo.AppendLine($"  Valor: {atendimento.Valor:N2}");

        corpo.AppendLine();
        corpo.AppendLine("O convite para a sua agenda está anexado (atendimento.ics).");
        corpo.AppendLine($"Ou adicione direto no Google Agenda: {link}");

        if (lembrete)
        {
            corpo.AppendLine();
            corpo.AppendLine(
                "Se precisar remarcar, responda este e-mail ou ligue para a recepção.");
        }

        return corpo.ToString();
    }

    private static string MontarSms(Atendimento atendimento, string link, bool lembrete)
        => lembrete
            ? "Clinica Vida: lembrete do seu atendimento amanha "
              + $"{atendimento.DataHora:dd/MM} as {atendimento.DataHora:HH:mm} com "
              + $"{atendimento.Medico.NomeCompleto}."
            : "Clinica Vida: atendimento confirmado para "
              + $"{atendimento.DataHora:dd/MM} as {atendimento.DataHora:HH:mm} com "
              + $"{atendimento.Medico.NomeCompleto}. Agenda: {link}";

    private static string DiaDaSemana(DateTime data) => data.DayOfWeek switch
    {
        DayOfWeek.Monday => "segunda-feira",
        DayOfWeek.Tuesday => "terça-feira",
        DayOfWeek.Wednesday => "quarta-feira",
        DayOfWeek.Thursday => "quinta-feira",
        DayOfWeek.Friday => "sexta-feira",
        DayOfWeek.Saturday => "sábado",
        _ => "domingo"
    };
}

// ================================== Handlers ==================================

public class ExecutarRodadaDeLembretesHandler
    : IRequestHandler<ExecutarRodadaDeLembretes, RodadaLembretes>
{
    private readonly IAtendimentoRepositorio _repositorio;
    private readonly MontadorDeMensagens _montador;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExecutarRodadaDeLembretesHandler> _logger;

    public ExecutarRodadaDeLembretesHandler(
        IAtendimentoRepositorio repositorio,
        MontadorDeMensagens montador,
        IUnitOfWork unitOfWork,
        ILogger<ExecutarRodadaDeLembretesHandler> logger)
    {
        _repositorio = repositorio;
        _montador = montador;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<RodadaLembretes> Handle(
        ExecutarRodadaDeLembretes comando, CancellationToken cancellationToken)
    {
        var pendentes = await _repositorio.ListarParaLembreteAsync(cancellationToken);
        var enviadas = new List<Notificacao>();
        var notificados = 0;

        foreach (var atendimento in pendentes)
        {
            // Uma falha num paciente não pode interromper a rodada inteira.
            try
            {
                enviadas.AddRange(_montador.Enviar(atendimento, lembrete: true));

                // O carimbo é o que torna a rodada idempotente: chamar duas
                // vezes no mesmo dia não manda duas mensagens.
                atendimento.RegistrarLembreteEnviado();
                notificados++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Falha ao enviar lembrete do atendimento {AtendimentoId}", atendimento.Id);
            }
        }

        if (notificados > 0) await _unitOfWork.CommitAsync(cancellationToken);

        return new RodadaLembretes(DateTime.Now, pendentes.Count, enviadas);
    }
}

public class ReenviarConfirmacaoHandler : IRequestHandler<ReenviarConfirmacao, List<Notificacao>>
{
    private readonly IAtendimentoRepositorio _repositorio;
    private readonly MontadorDeMensagens _montador;

    public ReenviarConfirmacaoHandler(
        IAtendimentoRepositorio repositorio, MontadorDeMensagens montador)
    {
        _repositorio = repositorio;
        _montador = montador;
    }

    public async Task<List<Notificacao>> Handle(
        ReenviarConfirmacao comando, CancellationToken cancellationToken)
    {
        var atendimento = await _repositorio.ObterPorIdAsync(
                              comando.AtendimentoId, cancellationToken)
                          ?? throw new RegraNegocioException(
                              $"Atendimento {comando.AtendimentoId} não encontrado.");

        if (!atendimento.EstaAberto)
            throw new RegraNegocioException(
                $"Não é possível enviar confirmação de um atendimento {atendimento.Status}.");

        return _montador.Enviar(atendimento, lembrete: false);
    }
}

public class ListarNotificacoesHandler : IRequestHandler<ListarNotificacoes, List<Notificacao>>
{
    private readonly IEnviadorDeNotificacao _enviador;

    public ListarNotificacoesHandler(IEnviadorDeNotificacao enviador) => _enviador = enviador;

    public Task<List<Notificacao>> Handle(
        ListarNotificacoes consulta, CancellationToken cancellationToken)
        => Task.FromResult(_enviador.Listar(consulta.AtendimentoId));
}

public class ObterNotificacaoHandler : IRequestHandler<ObterNotificacao, Notificacao?>
{
    private readonly IEnviadorDeNotificacao _enviador;

    public ObterNotificacaoHandler(IEnviadorDeNotificacao enviador) => _enviador = enviador;

    public Task<Notificacao?> Handle(
        ObterNotificacao consulta, CancellationToken cancellationToken)
        => Task.FromResult(_enviador.ObterPorId(consulta.Id));
}

public class ObterConviteIcsHandler : IRequestHandler<ObterConviteIcs, string>
{
    private readonly IAtendimentoRepositorio _repositorio;
    private readonly IGeradorDeConviteDeCalendario _calendario;

    public ObterConviteIcsHandler(
        IAtendimentoRepositorio repositorio, IGeradorDeConviteDeCalendario calendario)
    {
        _repositorio = repositorio;
        _calendario = calendario;
    }

    public async Task<string> Handle(
        ObterConviteIcs consulta, CancellationToken cancellationToken)
        => _calendario.GerarIcs(
            await _repositorio.ObterPorIdAsync(consulta.AtendimentoId, cancellationToken)
            ?? throw new RegraNegocioException(
                $"Atendimento {consulta.AtendimentoId} não encontrado."));
}

public class ObterLinkGoogleAgendaHandler : IRequestHandler<ObterLinkGoogleAgenda, string>
{
    private readonly IAtendimentoRepositorio _repositorio;
    private readonly IGeradorDeConviteDeCalendario _calendario;

    public ObterLinkGoogleAgendaHandler(
        IAtendimentoRepositorio repositorio, IGeradorDeConviteDeCalendario calendario)
    {
        _repositorio = repositorio;
        _calendario = calendario;
    }

    public async Task<string> Handle(
        ObterLinkGoogleAgenda consulta, CancellationToken cancellationToken)
        => _calendario.GerarLinkGoogleAgenda(
            await _repositorio.ObterPorIdAsync(consulta.AtendimentoId, cancellationToken)
            ?? throw new RegraNegocioException(
                $"Atendimento {consulta.AtendimentoId} não encontrado."));
}
