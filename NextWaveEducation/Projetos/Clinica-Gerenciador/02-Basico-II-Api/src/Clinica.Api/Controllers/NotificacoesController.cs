using Clinica.Api.Seguranca;
using Clinica.Api.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Controllers;

/// <summary>
/// Caixa de saída das notificações e disparo manual da rodada de lembretes
/// (útil para testar sem esperar o background service).
/// </summary>
[ApiController]
[Route("api/notificacoes")]
[Authorize(Roles = Perfis.Todos)]
public class NotificacoesController : ControllerBase
{
    private readonly CaixaDeSaidaNotificacoes _caixaDeSaida;
    private readonly NotificacaoService _notificacaoService;

    public NotificacoesController(
        CaixaDeSaidaNotificacoes caixaDeSaida, NotificacaoService notificacaoService)
    {
        _caixaDeSaida = caixaDeSaida;
        _notificacaoService = notificacaoService;
    }

    /// <summary>Notificações enviadas, podendo filtrar por atendimento.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<Notificacao>), StatusCodes.Status200OK)]
    public IActionResult Listar([FromQuery] int? atendimentoId)
        => Ok(_caixaDeSaida.Listar(atendimentoId));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Notificacao), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ObterPorId(int id)
    {
        var notificacao = _caixaDeSaida.BuscarPorId(id);

        return notificacao is null
            ? NotFound(new { mensagem = $"Notificação {id} não encontrada." })
            : Ok(notificacao);
    }

    /// <summary>Reenvia a confirmação sem mexer no status do atendimento.</summary>
    [HttpPost("reenviar-confirmacao/{atendimentoId:int}")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(List<Notificacao>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReenviarConfirmacao(int atendimentoId)
        => Ok(await _notificacaoService.ReenviarConfirmacaoAsync(atendimentoId));

    /// <summary>
    /// Roda agora a mesma rotina do background service. Chamar duas vezes
    /// seguidas não manda nada na segunda: o lembrete já ficou carimbado.
    /// </summary>
    [HttpPost("rodada-de-lembretes")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(RodadaLembretes), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExecutarRodadaDeLembretes()
        => Ok(await _notificacaoService.ExecutarRodadaDeLembretesAsync());
}
