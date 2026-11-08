using BancoSangue.Api.Middlewares;
using BancoSangue.Application.Common;
using BancoSangue.Application.Doacoes.Commands.RegistrarDoacao;
using BancoSangue.Application.Doacoes.Queries.ListarDoacoes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BancoSangue.Api.Controllers;

[ApiController]
[Route("api/doacoes")]
[Produces("application/json")]
public class DoacoesController : ControllerBase
{
    private readonly ISender _sender;

    public DoacoesController(ISender sender) => _sender = sender;

    /// <summary>Lista todas as doações registradas, com os dados dos doadores.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<DoacaoComDoadorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ListarDoacoesQuery(), cancellationToken));

    /// <summary>
    /// Registra uma doação. O estoque é atualizado por um evento de domínio
    /// e a resposta inclui alerta quando o estoque fica abaixo do mínimo.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResultadoDoacaoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarDoacaoCommand command,
        CancellationToken cancellationToken)
    {
        var resultado = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Listar), resultado);
    }
}
