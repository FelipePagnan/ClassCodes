using Clinica.Dominio.Entidades;

namespace Clinica.Aplicacao.Contratos;

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
/// Envio de mensagem ao paciente. A aplicação declara o contrato; quem
/// implementa é a infraestrutura — hoje uma caixa de saída em memória,
/// amanhã SMTP e um gateway de SMS, sem nada mudar aqui.
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
/// Geração do convite de calendário. Fica atrás de interface porque é
/// detalhe de formato: o .ics de hoje poderia ser uma chamada à API do
/// Google amanhã.
/// </summary>
public interface IGeradorDeConviteDeCalendario
{
    /// <summary>Convite no formato iCalendar (RFC 5545).</summary>
    string GerarIcs(Atendimento atendimento);

    /// <summary>Link que abre o Google Agenda com o evento preenchido.</summary>
    string GerarLinkGoogleAgenda(Atendimento atendimento);
}

/// <summary>Emissão do token de acesso.</summary>
public interface IGeradorDeToken
{
    (string Token, DateTime ExpiraEm) Gerar(Usuario usuario);
}

/// <summary>
/// Quem está autenticado na requisição atual. A aplicação precisa disso para
/// filtrar a agenda do médico, mas não pode conhecer HttpContext — então o
/// contrato fica aqui e a API o implementa.
/// </summary>
public interface IUsuarioAtual
{
    int? UsuarioId { get; }
    string? Nome { get; }
    string? Perfil { get; }

    /// <summary>Preenchido só quando o perfil é Medico.</summary>
    int? MedicoId { get; }

    bool EhMedico { get; }

    /// <summary>
    /// Médico vê só a própria agenda; administrador e recepcionista veem
    /// todas. Devolve o filtro de médico a aplicar, ou null.
    /// </summary>
    int? FiltroDeMedico { get; }
}
