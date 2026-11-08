using Clinica.Aplicacao.Contratos;
using Microsoft.Extensions.Logging;

namespace Clinica.Infraestrutura.Notificacoes;

/// <summary>
/// Caixa de saída em memória: faz o papel do servidor de e-mail e do gateway
/// de SMS. A montagem da mensagem, o .ics e o link do Google Agenda são
/// reais; só o transporte é simulado.
///
/// Trocar por envio de verdade é registrar outra implementação de
/// IEnviadorDeNotificacao no Program. Nenhum handler muda — é exatamente o
/// que a inversão de dependência compra.
///
/// Registrada como singleton, então as travas protegem acesso concorrente:
/// o background service e as requisições HTTP escrevem aqui ao mesmo tempo.
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
