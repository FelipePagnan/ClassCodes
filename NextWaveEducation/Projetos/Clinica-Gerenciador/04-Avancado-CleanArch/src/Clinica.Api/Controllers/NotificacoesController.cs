using Clinica.Api.Seguranca;
using Clinica.Aplicacao.Contratos;
using Clinica.Aplicacao.Notificacoes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Controllers;

/// <summary>
/// Caixa de saída das notificações e disparo manual da rodada de lembretes,
/// útil para testar sem esperar o background service.
/// </summary>
[ApiController]
[Route("api/notificacoes")]
[Authorize(Roles = Perfis.Todos)]
public class NotificacoesController : ControllerBase
{
    private readonly ISender _sender;

    public NotificacoesController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof(List<Notificacao>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] int? atendimentoId, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ListarNotificacoes(atendimentoId), cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Notificacao), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
    {
        var notificacao = await _sender.Send(new ObterNotificacao(id), cancellationToken);

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
        => Ok(await _sender.Send(new ReenviarConfirmacao(atendimentoId), cancellationToken));

    /// <summary>
    /// Roda agora o mesmo comando que o background service dispara. Chamar
    /// duas vezes seguidas não manda nada na segunda: o lembrete já ficou
    /// carimbado.
    /// </summary>
    [HttpPost("rodada-de-lembretes")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(RodadaLembretes), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExecutarRodada(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ExecutarRodadaDeLembretes(), cancellationToken));
}
