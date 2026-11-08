using System.Text;
using Clinica.Api.Entidades;
using Clinica.Api.Excecoes;
using Clinica.Api.Util;

namespace Clinica.Api.Servicos;

/// <summary>Canal pelo qual uma notificação saiu.</summary>
public enum CanalNotificacao { Email = 1, Sms = 2 }

/// <summary>Registro de uma notificação enviada.</summary>
public record Notificacao(
    int Id,
    int AtendimentoId,
    CanalNotificacao Canal,
    string Destinatario,
    string Assunto,
    string Corpo,
    DateTime EnviadaEm,
    string? LinkGoogleAgenda = null,
    string? AnexoIcs = null);

/// <summary>
/// Caixa de saída em memória, registrada como singleton. Substitui o SMTP e
/// o gateway de SMS que entrariam num ambiente real — a montagem da mensagem,
/// o .ics e o link do Google Agenda são de verdade; só o transporte é falso.
/// </summary>
public class CaixaDeSaidaNotificacoes
{
    private readonly List<Notificacao> _notificacoes = new();
    private readonly object _trava = new();
    private int _proximoId = 1;

    public Notificacao Registrar(
        int atendimentoId,
        CanalNotificacao canal,
        string destinatario,
        string assunto,
        string corpo,
        string? link = null,
        string? ics = null)
    {
        lock (_trava)
        {
            var notificacao = new Notificacao(
                _proximoId++, atendimentoId, canal, destinatario,
                assunto, corpo, DateTime.Now, link, ics);

            _notificacoes.Add(notificacao);
            return notificacao;
        }
    }

    public List<Notificacao> Listar(int? atendimentoId = null)
    {
        lock (_trava)
        {
            var consulta = _notificacoes.AsEnumerable();

            if (atendimentoId is not null)
                consulta = consulta.Where(n => n.AtendimentoId == atendimentoId);

            return consulta.OrderByDescending(n => n.EnviadaEm).ToList();
        }
    }

    public Notificacao? BuscarPorId(int id)
    {
        lock (_trava)
        {
            return _notificacoes.FirstOrDefault(n => n.Id == id);
        }
    }
}

/// <summary>
/// Confirmação de agendamento (e-mail com .ics + SMS com link do Google
/// Agenda) e lembrete do dia anterior.
/// </summary>
public class NotificacaoService
{
    private readonly AtendimentoService _atendimentoService;
    private readonly CaixaDeSaidaNotificacoes _caixaDeSaida;
    private readonly ILogger<NotificacaoService> _logger;

    public NotificacaoService(
        AtendimentoService atendimentoService,
        CaixaDeSaidaNotificacoes caixaDeSaida,
        ILogger<NotificacaoService> logger)
    {
        _atendimentoService = atendimentoService;
        _caixaDeSaida = caixaDeSaida;
        _logger = logger;
    }

    // ----------------------------- Confirmação -----------------------------

    public async Task<List<Notificacao>> EnviarConfirmacaoAsync(int atendimentoId)
    {
        var atendimento = await _atendimentoService.ConfirmarAsync(atendimentoId);
        return Enviar(atendimento, lembrete: false);
    }

    /// <summary>
    /// Reenvia a confirmação de um atendimento que já está confirmado, sem
    /// mexer no status.
    /// </summary>
    public async Task<List<Notificacao>> ReenviarConfirmacaoAsync(int atendimentoId)
    {
        var atendimento = await _atendimentoService.ObterPorIdAsync(atendimentoId);

        if (!atendimento.EstaAberto)
            throw new RegraNegocioException(
                $"Não é possível enviar confirmação de um atendimento {atendimento.Status}.");

        return Enviar(atendimento, lembrete: false);
    }

    // ------------------------------- Lembretes -------------------------------

    /// <summary>
    /// Rodada de lembretes: notifica os atendimentos de amanhã que ainda não
    /// receberam lembrete. É o que o background service chama em laço.
    /// </summary>
    public async Task<RodadaLembretes> ExecutarRodadaDeLembretesAsync()
    {
        var pendentes = await _atendimentoService.ListarParaLembreteAsync();
        var enviadas = new List<Notificacao>();

        foreach (var atendimento in pendentes)
        {
            // Um erro num paciente não pode interromper a rodada inteira.
            try
            {
                enviadas.AddRange(Enviar(atendimento, lembrete: true));
                await _atendimentoService.RegistrarLembreteEnviadoAsync(atendimento.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Falha ao enviar lembrete do atendimento {AtendimentoId}", atendimento.Id);
            }
        }

        return new RodadaLembretes(DateTime.Now, pendentes.Count, enviadas);
    }

    // --------------------------------- Envio ---------------------------------

    private List<Notificacao> Enviar(Atendimento atendimento, bool lembrete)
    {
        var paciente = atendimento.Paciente
            ?? throw new RegraNegocioException("O atendimento está sem paciente.");

        var link = CalendarioIcs.GerarLinkGoogleAgenda(atendimento);
        var ics = CalendarioIcs.Gerar(atendimento);
        var enviadas = new List<Notificacao>();

        if (!string.IsNullOrWhiteSpace(paciente.Email))
        {
            var assunto = lembrete
                ? $"Lembrete: seu atendimento é amanhã às {atendimento.DataHora:HH:mm}"
                : $"Confirmação do seu atendimento em {atendimento.DataHora:dd/MM}";

            enviadas.Add(_caixaDeSaida.Registrar(
                atendimento.Id, CanalNotificacao.Email, paciente.Email!,
                assunto, MontarEmail(atendimento, link, lembrete), link, ics));
        }

        enviadas.Add(_caixaDeSaida.Registrar(
            atendimento.Id, CanalNotificacao.Sms, paciente.CelularFormatado,
            lembrete ? "Lembrete de atendimento" : "Confirmação de atendimento",
            MontarSms(atendimento, link, lembrete), link));

        _logger.LogInformation(
            "{Quantidade} notificação(ões) de {Tipo} enviada(s) para o atendimento {AtendimentoId}",
            enviadas.Count, lembrete ? "lembrete" : "confirmação", atendimento.Id);

        return enviadas;
    }

    private static string MontarEmail(Atendimento atendimento, string link, bool lembrete)
    {
        var corpo = new StringBuilder();
        corpo.AppendLine($"Olá, {PrimeiroNome(atendimento.Paciente?.NomeCompleto)}!");
        corpo.AppendLine();

        if (lembrete)
        {
            corpo.AppendLine("Passando para lembrar do seu atendimento de amanhã, " +
                             $"{atendimento.DataHora:dd/MM/yyyy}, às {atendimento.DataHora:HH:mm}.");
        }
        else
        {
            corpo.AppendLine("Seu atendimento está confirmado:");
        }

        corpo.AppendLine();
        corpo.AppendLine($"  Serviço: {atendimento.Servico?.Nome}");
        corpo.AppendLine($"  Profissional: {atendimento.Medico?.NomeCompleto} " +
                         $"({atendimento.Medico?.CrmCompleto})");
        corpo.AppendLine($"  Data: {atendimento.DataHora:dd/MM/yyyy} " +
                         $"({DiaDaSemana(atendimento.DataHora)})");
        corpo.AppendLine($"  Horário: {atendimento.DataHora:HH:mm} às " +
                         $"{atendimento.DataHoraFim:HH:mm}");

        if (!lembrete)
            corpo.AppendLine($"  Valor: {atendimento.Valor:N2}");

        corpo.AppendLine();
        corpo.AppendLine("O convite para a sua agenda está anexado (atendimento.ics).");
        corpo.AppendLine($"Ou adicione direto no Google Agenda: {link}");

        if (lembrete)
        {
            corpo.AppendLine();
            corpo.AppendLine("Se precisar remarcar, responda este e-mail ou ligue para a recepção.");
        }

        return corpo.ToString();
    }

    private static string MontarSms(Atendimento atendimento, string link, bool lembrete)
        => lembrete
            ? $"Clinica Vida: lembrete do seu atendimento amanha " +
              $"{atendimento.DataHora:dd/MM} as {atendimento.DataHora:HH:mm} com " +
              $"{atendimento.Medico?.NomeCompleto}."
            : $"Clinica Vida: atendimento confirmado para " +
              $"{atendimento.DataHora:dd/MM} as {atendimento.DataHora:HH:mm} com " +
              $"{atendimento.Medico?.NomeCompleto}. Agenda: {link}";

    private static string PrimeiroNome(string? nomeCompleto)
        => string.IsNullOrWhiteSpace(nomeCompleto)
            ? "paciente"
            : nomeCompleto.Trim().Split(' ')[0];

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

public record RodadaLembretes(
    DateTime ExecutadaEm,
    int AtendimentosEncontrados,
    List<Notificacao> Enviadas);
