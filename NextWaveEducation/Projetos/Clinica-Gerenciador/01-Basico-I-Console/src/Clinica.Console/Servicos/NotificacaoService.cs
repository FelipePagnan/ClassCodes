using System.Text;
using Clinica.Console.Entidades;
using Clinica.Console.Excecoes;
using Clinica.Console.Util;

namespace Clinica.Console.Servicos;

/// <summary>Canal pelo qual uma notificação saiu.</summary>
public enum CanalNotificacao { Email = 1, Sms = 2 }

/// <summary>Registro de uma notificação enviada (a "caixa de saída").</summary>
public record Notificacao(
    int Id,
    int AtendimentoId,
    CanalNotificacao Canal,
    string Destinatario,
    string Assunto,
    string Corpo,
    DateTime EnviadaEm,
    string? AnexoIcs = null,
    string? LinkGoogleAgenda = null);

/// <summary>
/// Confirmação de agendamento e lembrete do dia anterior.
///
/// No Console não há servidor de SMTP nem gateway de SMS, então o envio é
/// gravado numa caixa de saída em memória e impresso na tela. A montagem da
/// mensagem, o .ics e o link do Google Agenda são reais e idênticos aos dos
/// níveis seguintes — lá só o transporte muda.
/// </summary>
public class NotificacaoService
{
    private readonly AtendimentoService _atendimentoService;

    private readonly List<Notificacao> _enviadas = new();
    private int _proximoId = 1;

    public NotificacaoService(AtendimentoService atendimentoService)
        => _atendimentoService = atendimentoService;

    public IReadOnlyList<Notificacao> CaixaDeSaida()
        => _enviadas.OrderByDescending(n => n.EnviadaEm).ToList().AsReadOnly();

    public IEnumerable<Notificacao> ListarPorAtendimento(int atendimentoId)
        => CaixaDeSaida().Where(n => n.AtendimentoId == atendimentoId);

    // --------------------------- Confirmação ---------------------------

    /// <summary>
    /// Confirma o agendamento e envia a confirmação por e-mail (com .ics
    /// anexado) e por SMS (com o link do Google Agenda).
    /// </summary>
    public List<Notificacao> EnviarConfirmacao(int atendimentoId)
    {
        var atendimento = _atendimentoService.ObterPorId(atendimentoId);

        if (!atendimento.EstaAberto)
            throw new RegraNegocioException(
                $"Não é possível enviar confirmação de um atendimento {atendimento.Status}.");

        // Confirmar() já carimba ConfirmacaoEnviadaEm e muda o status.
        if (atendimento.Status == Enums.StatusAtendimento.Agendado)
            atendimento.Confirmar();

        var ics = CalendarioIcs.Gerar(atendimento);
        var link = CalendarioIcs.GerarLinkGoogleAgenda(atendimento);

        var enviadas = new List<Notificacao>();

        var paciente = atendimento.Paciente
            ?? throw new RegraNegocioException("O atendimento está sem paciente.");

        if (!string.IsNullOrWhiteSpace(paciente.Email))
        {
            enviadas.Add(Registrar(
                atendimento.Id,
                CanalNotificacao.Email,
                paciente.Email!,
                $"Confirmação do seu atendimento em {atendimento.DataHora:dd/MM}",
                MontarCorpoConfirmacao(atendimento, link),
                ics,
                link));
        }

        enviadas.Add(Registrar(
            atendimento.Id,
            CanalNotificacao.Sms,
            paciente.CelularFormatado,
            "Confirmação de atendimento",
            MontarSmsConfirmacao(atendimento, link),
            null,
            link));

        return enviadas;
    }

    // ---------------------------- Lembretes ----------------------------

    /// <summary>
    /// Roda a rodada de lembretes: pega os atendimentos de amanhã que ainda
    /// não receberam lembrete e notifica. É exatamente o que o
    /// BackgroundService dos níveis II, III e IV faz em laço.
    /// </summary>
    public RodadaLembretes ExecutarRodadaDeLembretes()
    {
        var pendentes = _atendimentoService.ListarParaLembrete().ToList();
        var enviadas = new List<Notificacao>();

        foreach (var atendimento in pendentes)
        {
            // Um erro num paciente não pode interromper a rodada inteira.
            try
            {
                enviadas.AddRange(EnviarLembrete(atendimento));
            }
            catch (RegraNegocioException)
            {
                continue;
            }
        }

        return new RodadaLembretes(DateTime.Now, pendentes.Count, enviadas);
    }

    private List<Notificacao> EnviarLembrete(Atendimento atendimento)
    {
        var paciente = atendimento.Paciente
            ?? throw new RegraNegocioException("O atendimento está sem paciente.");

        var link = CalendarioIcs.GerarLinkGoogleAgenda(atendimento);
        var enviadas = new List<Notificacao>();

        if (!string.IsNullOrWhiteSpace(paciente.Email))
        {
            enviadas.Add(Registrar(
                atendimento.Id,
                CanalNotificacao.Email,
                paciente.Email!,
                $"Lembrete: seu atendimento é amanhã às {atendimento.DataHora:HH:mm}",
                MontarCorpoLembrete(atendimento, link),
                CalendarioIcs.Gerar(atendimento),
                link));
        }

        enviadas.Add(Registrar(
            atendimento.Id,
            CanalNotificacao.Sms,
            paciente.CelularFormatado,
            "Lembrete de atendimento",
            MontarSmsLembrete(atendimento),
            null,
            link));

        // Carimba só depois do envio: se der erro acima, o próximo ciclo tenta de novo.
        atendimento.RegistrarLembreteEnviado();

        return enviadas;
    }

    // ------------------------- Montagem das mensagens -------------------------

    private static string MontarCorpoConfirmacao(Atendimento atendimento, string linkAgenda)
    {
        var corpo = new StringBuilder();
        corpo.AppendLine($"Olá, {PrimeiroNome(atendimento.Paciente?.NomeCompleto)}!");
        corpo.AppendLine();
        corpo.AppendLine("Seu atendimento está confirmado:");
        corpo.AppendLine();
        corpo.AppendLine($"  Serviço: {atendimento.Servico?.Nome}");
        corpo.AppendLine($"  Profissional: {atendimento.Medico?.NomeCompleto} " +
                         $"({atendimento.Medico?.CrmCompleto})");
        corpo.AppendLine($"  Data: {atendimento.DataHora:dd/MM/yyyy} " +
                         $"({DiaDaSemana(atendimento.DataHora)})");
        corpo.AppendLine($"  Horário: {atendimento.DataHora:HH:mm} às " +
                         $"{atendimento.DataHoraFim:HH:mm}");
        corpo.AppendLine($"  Valor: {atendimento.Valor:N2}");
        corpo.AppendLine();
        corpo.AppendLine("O convite para a sua agenda está anexado a este e-mail (atendimento.ics).");
        corpo.AppendLine($"Ou adicione direto no Google Agenda: {linkAgenda}");
        corpo.AppendLine();
        corpo.AppendLine("Enviaremos um lembrete um dia antes. Até lá!");

        return corpo.ToString();
    }

    private static string MontarSmsConfirmacao(Atendimento atendimento, string linkAgenda)
        => $"Clinica Vida: atendimento confirmado para " +
           $"{atendimento.DataHora:dd/MM} as {atendimento.DataHora:HH:mm} com " +
           $"{atendimento.Medico?.NomeCompleto}. Agenda: {linkAgenda}";

    private static string MontarCorpoLembrete(Atendimento atendimento, string linkAgenda)
    {
        var corpo = new StringBuilder();
        corpo.AppendLine($"Olá, {PrimeiroNome(atendimento.Paciente?.NomeCompleto)}!");
        corpo.AppendLine();
        corpo.AppendLine($"Passando para lembrar do seu atendimento de amanhã, " +
                         $"{atendimento.DataHora:dd/MM/yyyy}, às {atendimento.DataHora:HH:mm}.");
        corpo.AppendLine();
        corpo.AppendLine($"  Serviço: {atendimento.Servico?.Nome}");
        corpo.AppendLine($"  Profissional: {atendimento.Medico?.NomeCompleto}");
        corpo.AppendLine();
        corpo.AppendLine("Se precisar remarcar, responda este e-mail ou ligue para a recepção.");
        corpo.AppendLine($"Evento na agenda: {linkAgenda}");

        return corpo.ToString();
    }

    private static string MontarSmsLembrete(Atendimento atendimento)
        => $"Clinica Vida: lembrete do seu atendimento amanha " +
           $"{atendimento.DataHora:dd/MM} as {atendimento.DataHora:HH:mm} com " +
           $"{atendimento.Medico?.NomeCompleto}.";

    private Notificacao Registrar(
        int atendimentoId,
        CanalNotificacao canal,
        string destinatario,
        string assunto,
        string corpo,
        string? ics,
        string? link)
    {
        var notificacao = new Notificacao(
            _proximoId++, atendimentoId, canal, destinatario,
            assunto, corpo, DateTime.Now, ics, link);

        _enviadas.Add(notificacao);
        return notificacao;
    }

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
