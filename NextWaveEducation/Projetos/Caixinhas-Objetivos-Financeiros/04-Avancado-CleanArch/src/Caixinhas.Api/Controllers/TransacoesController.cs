using Caixinhas.Api.Middlewares;
using Caixinhas.Application.Common;
using Caixinhas.Application.Transacoes.Commands.CadastrarTransacao;
using Caixinhas.Application.Transacoes.Commands.RemoverTransacao;
using Caixinhas.Application.Transacoes.Queries.ListarTransacoes;
using Caixinhas.Application.Transacoes.Queries.ObterExtrato;
using Caixinhas.Application.Transacoes.Queries.ObterTransacaoPorId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Caixinhas.Api.Controllers;

/// <summary>Cadastro, listagem, detalhes e remoção de transações.</summary>
[ApiController]
[Route("api/objetivos/{idObjetivo:int}/transacoes")]
[Produces("application/json")]
public class TransacoesController : ControllerBase
{
    private readonly ISender _sender;

    public TransacoesController(ISender sender) => _sender = sender;

    /// <summary>Lista as transações ativas da caixinha.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<TransacaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Listar(int idObjetivo, CancellationToken ct)
        => Ok(await _sender.Send(new ListarTransacoesQuery(idObjetivo), ct));

    /// <summary>Detalhes de uma transação.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TransacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int idObjetivo, int id, CancellationToken ct)
        => Ok(await _sender.Send(new ObterTransacaoPorIdQuery(idObjetivo, id), ct));

    /// <summary>
    /// Cadastra uma transação. O ValorTotal da caixinha é atualizado pela
    /// entidade e gravado junto, dentro de uma transação de banco.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResultadoTransacaoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cadastrar(
        int idObjetivo, [FromBody] CadastrarTransacaoCommand command, CancellationToken ct)
    {
        var resultado = await _sender.Send(command with { IdObjetivo = idObjetivo }, ct);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { idObjetivo, id = resultado.Transacao.Id },
            resultado);
    }

    /// <summary>
    /// Remoção lógica da transação. O ValorTotal da caixinha é ajustado na
    /// mesma transação de banco.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int idObjetivo, int id, CancellationToken ct)
    {
        await _sender.Send(new RemoverTransacaoCommand(idObjetivo, id), ct);
        return NoContent();
    }

    /// <summary>Extrato da caixinha com o saldo corrido após cada movimentação.</summary>
    [HttpGet("extrato")]
    [ProducesResponseType(typeof(List<LinhaExtratoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Extrato(int idObjetivo, CancellationToken ct)
        => Ok(await _sender.Send(new ObterExtratoQuery(idObjetivo), ct));
}
