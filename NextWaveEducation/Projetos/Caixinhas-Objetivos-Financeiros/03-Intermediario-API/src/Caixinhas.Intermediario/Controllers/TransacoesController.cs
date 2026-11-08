using Caixinhas.Intermediario.Aplicacao.InputModels;
using Caixinhas.Intermediario.Aplicacao.Servicos;
using Caixinhas.Intermediario.Aplicacao.ViewModels;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Caixinhas.Intermediario.Controllers;

/// <summary>Cadastro, listagem, detalhes e remoção de transações.</summary>
[ApiController]
[Route("api/objetivos/{idObjetivo:int}/transacoes")]
[Produces("application/json")]
public class TransacoesController : ControllerBase
{
    private readonly ITransacaoAppService _transacaoAppService;
    private readonly IValidator<CriarTransacaoInputModel> _validador;

    public TransacoesController(
        ITransacaoAppService transacaoAppService,
        IValidator<CriarTransacaoInputModel> validador)
    {
        _transacaoAppService = transacaoAppService;
        _validador = validador;
    }

    /// <summary>Lista as transações ativas da caixinha.</summary>
    [HttpGet]
    public async Task<ActionResult<List<TransacaoViewModel>>> Listar(int idObjetivo)
        => Ok(await _transacaoAppService.ListarPorObjetivoAsync(idObjetivo));

    /// <summary>Detalhes de uma transação.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TransacaoViewModel>> ObterPorId(int idObjetivo, int id)
        => Ok(await _transacaoAppService.ObterPorIdAsync(idObjetivo, id));

    /// <summary>
    /// Cadastra uma transação. O ValorTotal da caixinha é atualizado na mesma
    /// operação, dentro de uma transação de banco.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ResultadoTransacaoViewModel>> Cadastrar(
        int idObjetivo, [FromBody] CriarTransacaoInputModel input)
    {
        await _validador.ValidateAndThrowAsync(input);

        var resultado = await _transacaoAppService.CadastrarAsync(idObjetivo, input);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { idObjetivo, id = resultado.Transacao.Id },
            resultado);
    }

    /// <summary>
    /// Remoção lógica da transação. O ValorTotal da caixinha é ajustado.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int idObjetivo, int id)
    {
        await _transacaoAppService.RemoverAsync(idObjetivo, id);
        return NoContent();
    }

    /// <summary>Extrato da caixinha com o saldo corrido após cada movimentação.</summary>
    [HttpGet("extrato")]
    public async Task<ActionResult<List<LinhaExtratoViewModel>>> Extrato(int idObjetivo)
        => Ok(await _transacaoAppService.GerarExtratoAsync(idObjetivo));
}
