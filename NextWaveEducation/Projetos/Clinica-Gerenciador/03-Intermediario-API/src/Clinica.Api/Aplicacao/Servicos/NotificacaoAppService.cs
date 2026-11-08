using System.Text;
using Clinica.Api.Dominio.Entidades;
using Clinica.Api.Dominio.Excecoes;
using Clinica.Api.Infraestrutura.Repositorios;
using Clinica.Api.Util;

namespace Clinica.Api.Aplicacao.Servicos;

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

/// <summary>Resultado de uma rodada de lembretes.</summary>
public record RodadaLembretes(
    DateTime ExecutadaEm,
    int AtendimentosEncontrados,
    List<Notificacao> Enviadas);

/// <summary>
/// Contrato de envio. Com a interface no meio, trocar a caixa de saída em
/// memória por SMTP e um gateway de SMS é registrar outra implementação no
/// Program — nada no caso de uso muda.
/// </summary>
public interface IEnviadorDeNotificacao
{
    Notificacao Enviar(
        int atendimentoId,
        CanalNotificacao canal,
        string destinatario,
        string assunto,
        string corpo,
        string? link = null,
        string? ics = null);

    List<Notificacao> Listar(int? atendimentoId = null);

    Notificacao? ObterPorId(int id);
}

/// <summary>
/// Implementação em memória, registrada como singleton. Faz o papel do
/// servidor de e-mail e do gateway de SMS. A montagem da mensagem, o .ics e
/// o link do Google Agenda são reais; só o transporte é simulado.
/// </summary>
public class CaixaDeSaidaEmMemoria : IEnviadorDeNotificacao
{
    private readonly List<Notificacao> _notificacoes = new();
    private readonly object _trava = new();
    private readonly ILogger<CaixaDeSaidaEmMemoria> _logger;
    private int _proximoId = 1;

    public CaixaDeSaidaEmMemoria(ILogger<CaixaDeSaidaEmMemoria> logger) => _logger = logger;

    public Notificacao Enviar(
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

            _logger.LogInformation(
                "[{Canal}] para {Destinatario}: {Assunto}", canal, destinatario, assunto);

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

    public Notificacao? ObterPorId(int id)
    {
        lock (_trava)
        {
            return _notificacoes.FirstOrDefault(n => n.Id == id);
        }
    }
}

/// <summary>
/// Confirmação de agendamento (e-mail com .ics anexado + SMS com link do
/// Google Agenda) e lembrete do dia anterior.
/// </summary>
public class NotificacaoAppService
{
    private readonly IAtendimentoRepositorio _repositorio;
    private readonly IEnviadorDeNotificacao _enviador;
    private readonly ILogger<NotificacaoAppService> _logger;

    public NotificacaoAppService(
        IAtendimentoRepositorio repositorio,
        IEnviadorDeNotificacao enviador,
        ILogger<NotificacaoAppService> logger)
    {
        _repositorio = repositorio;
        _enviador = enviador;
        _logger = logger;
    }

    public List<Notificacao> ListarCaixaDeSaida(int? atendimentoId = null)
        => _enviador.Listar(atendimentoId);

    public Notificacao? ObterNotificacao(int id) => _enviador.ObterPorId(id);

    // ----------------------------- Google Agenda -----------------------------

    /// <summary>Convite .ics do atendimento, no formato iCalendar.</summary>
    public async Task<string> GerarIcsAsync(
        int atendimentoId, CancellationToken cancellationToken = default)
        => CalendarioIcs.Gerar(await ObterAsync(atendimentoId, cancellationToken));

    /// <summary>Link que abre o Google Agenda com o evento preenchido.</summary>
    public async Task<string> GerarLinkGoogleAgendaAsync(
        int atendimentoId, CancellationToken cancellationToken = default)
        => CalendarioIcs.GerarLinkGoogleAgenda(
            await ObterAsync(atendimentoId, cancellationToken));

    private async Task<Atendimento> ObterAsync(int id, CancellationToken cancellationToken)
        => await _repositorio.ObterPorIdAsync(id, cancellationToken)
           ?? throw new RegraNegocioException($"Atendimento {id} não encontrado.");

    // ------------------------------ Confirmação ------------------------------

    /// <summary>Confirma o atendimento e envia a confirmação nos dois canais.</summary>
    public async Task<List<Notificacao>> ConfirmarEEnviarAsync(
        int atendimentoId, CancellationToken cancellationToken = default)
    {
        var atendimento = await _repositorio.ObterPorIdAsync(atendimentoId, cancellationToken)
                          ?? throw new RegraNegocioException(
                              $"Atendimento {atendimentoId} não encontrado.");

        // Confirmar() já recusa qualquer status que não seja Agendado.
        atendimento.Confirmar();
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return Enviar(atendimento, lembrete: false);
    }

    /// <summary>Reenvia a confirmação sem mexer no status.</summary>
    public async Task<List<Notificacao>> ReenviarConfirmacaoAsync(
        int atendimentoId, CancellationToken cancellationToken = default)
    {
        var atendimento = await _repositorio.ObterPorIdAsync(atendimentoId, cancellationToken)
                          ?? throw new RegraNegocioException(
                              $"Atendimento {atendimentoId} não encontrado.");

        if (!atendimento.EstaAberto)
            throw new RegraNegocioException(
                $"Não é possível enviar confirmação de um atendimento {atendimento.Status}.");

        return Enviar(atendimento, lembrete: false);
    }

    // ------------------------------- Lembretes -------------------------------

    /// <summary>
    /// Rodada de lembretes: notifica os atendimentos de amanhã que ainda não
    /// receberam lembrete. É o que o background service chama em laço.
    ///
    /// O carimbo LembreteEnviadoEm é o que torna a operação idempotente:
    /// rodar duas vezes no mesmo dia não manda duas mensagens.
    /// </summary>
    public async Task<RodadaLembretes> ExecutarRodadaDeLembretesAsync(
        CancellationToken cancellationToken = default)
    {
        var pendentes = await _repositorio.ListarParaLembreteAsync(cancellationToken);
        var enviadas = new List<Notificacao>();

        foreach (var atendimento in pendentes)
        {
            // Uma falha num paciente não pode interromper a rodada inteira.
            try
            {
                enviadas.AddRange(Enviar(atendimento, lembrete: true));
                atendimento.RegistrarLembreteEnviado();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Falha ao enviar lembrete do atendimento {AtendimentoId}", atendimento.Id);
            }
        }

        if (enviadas.Count > 0)
            await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return new RodadaLembretes(DateTime.Now, pendentes.Count, enviadas);
    }

    // --------------------------------- Envio ---------------------------------

    private List<Notificacao> Enviar(Atendimento atendimento, bool lembrete)
    {
        var paciente = atendimento.Paciente;
        var link = CalendarioIcs.GerarLinkGoogleAgenda(atendimento);
        var ics = CalendarioIcs.Gerar(atendimento);
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
            ? "Passando para lembrar do seu atendimento de amanhã, " +
              $"{atendimento.DataHora:dd/MM/yyyy}, às {atendimento.DataHora:HH:mm}."
            : "Seu atendimento está confirmado:");

        corpo.AppendLine();
        corpo.AppendLine($"  Serviço: {atendimento.Servico.Nome}");
        corpo.AppendLine($"  Profissional: {atendimento.Medico.NomeCompleto} " +
                         $"({atendimento.Medico.Crm.Completo})");
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
            corpo.AppendLine(
                "Se precisar remarcar, responda este e-mail ou ligue para a recepção.");
        }

        return corpo.ToString();
    }

    private static string MontarSms(Atendimento atendimento, string link, bool lembrete)
        => lembrete
            ? "Clinica Vida: lembrete do seu atendimento amanha " +
              $"{atendimento.DataHora:dd/MM} as {atendimento.DataHora:HH:mm} com " +
              $"{atendimento.Medico.NomeCompleto}."
            : "Clinica Vida: atendimento confirmado para " +
              $"{atendimento.DataHora:dd/MM} as {atendimento.DataHora:HH:mm} com " +
              $"{atendimento.Medico.NomeCompleto}. Agenda: {link}";

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
