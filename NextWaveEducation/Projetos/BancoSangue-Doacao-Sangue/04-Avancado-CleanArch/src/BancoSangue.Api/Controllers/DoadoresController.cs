using BancoSangue.Api.Middlewares;
using BancoSangue.Application.Common;
using BancoSangue.Application.Doadores.Commands.CadastrarDoador;
using BancoSangue.Application.Doadores.Queries.ListarDoadores;
using BancoSangue.Application.Doadores.Queries.ObterDoadorPorId;
using BancoSangue.Application.Doadores.Queries.ObterHistoricoDoacoes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BancoSangue.Api.Controllers;

[ApiController]
[Route("api/doadores")]
[Produces("application/json")]
public class DoadoresController : ControllerBase
{
    private readonly ISender _sender;

    public DoadoresController(ISender sender) => _sender = sender;

    /// <summary>Lista todos os doadores cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<DoadorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ListarDoadoresQuery(), cancellationToken));

    /// <summary>Consulta um doador pelo id, com seu histórico de doações e aptidão atual.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DoadorDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ObterDoadorPorIdQuery(id), cancellationToken));

    /// <summary>Consulta o histórico de doações de um doador.</summary>
    [HttpGet("{id:int}/doacoes")]
    [ProducesResponseType(typeof(List<DoacaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> HistoricoDoacoes(int id, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ObterHistoricoDoacoesQuery(id), cancellationToken));

    /// <summary>
    /// Cadastra um novo doador. Informe o CEP para que o endereço seja
    /// preenchido automaticamente a partir da API ViaCEP.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DoadorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] CadastrarDoadorCommand command,
        CancellationToken cancellationToken)
    {
        var doador = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = doador.Id }, doador);
    }
}
