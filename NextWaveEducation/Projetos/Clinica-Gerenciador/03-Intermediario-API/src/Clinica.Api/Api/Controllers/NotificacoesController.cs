using Clinica.Api.Api.Seguranca;
using Clinica.Api.Aplicacao.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Api.Controllers;

/// <summary>
/// Caixa de saída das notificações e disparo manual da rodada de lembretes,
/// útil para testar sem esperar o background service.
/// </summary>
[ApiController]
[Route("api/notificacoes")]
[Authorize(Roles = Perfis.Todos)]
public class NotificacoesController : ControllerBase
{
    private readonly NotificacaoAppService _service;

    public NotificacoesController(NotificacaoAppService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(List<Notificacao>), StatusCodes.Status200OK)]
    public IActionResult Listar([FromQuery] int? atendimentoId)
        => Ok(_service.ListarCaixaDeSaida(atendimentoId));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Notificacao), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ObterPorId(int id)
    {
        var notificacao = _service.ObterNotificacao(id);

        return notificacao is null
            ? NotFound(new { mensagem = $"Notificação {id} não encontrada." })
            : Ok(notificacao);
    }

    /// <summary>Reenvia a confirmação sem mexer no status do atendimento.</summary>
    [HttpPost("reenviar-confirmacao/{atendimentoId:int}")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(List<Notificacao>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReenviarConfirmacao(
        int atendimentoId, CancellationToken cancellationToken)
        => Ok(await _service.ReenviarConfirmacaoAsync(atendimentoId, cancellationToken));

    /// <summary>
    /// Roda agora a mesma rotina do background service. Chamar duas vezes
    /// seguidas não manda nada na segunda: o lembrete já ficou carimbado.
    /// </summary>
    [HttpPost("rodada-de-lembretes")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(RodadaLembretes), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExecutarRodadaDeLembretes(CancellationToken cancellationToken)
        => Ok(await _service.ExecutarRodadaDeLembretesAsync(cancellationToken));
}
