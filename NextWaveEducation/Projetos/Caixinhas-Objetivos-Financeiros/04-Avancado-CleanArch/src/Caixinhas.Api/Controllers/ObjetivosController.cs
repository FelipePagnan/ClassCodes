using Caixinhas.Api.Middlewares;
using Caixinhas.Application.Common;
using Caixinhas.Application.Objetivos.Commands.AlterarStatus;
using Caixinhas.Application.Objetivos.Commands.AtualizarObjetivo;
using Caixinhas.Application.Objetivos.Commands.CadastrarObjetivo;
using Caixinhas.Application.Objetivos.Commands.EnviarCapa;
using Caixinhas.Application.Objetivos.Commands.RemoverObjetivo;
using Caixinhas.Application.Objetivos.Queries.ListarObjetivos;
using Caixinhas.Application.Objetivos.Queries.ObterCapa;
using Caixinhas.Application.Objetivos.Queries.ObterObjetivoPorId;
using Caixinhas.Application.Objetivos.Queries.SimularEvolucao;
using Caixinhas.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Caixinhas.Api.Controllers;

[ApiController]
[Route("api/objetivos")]
[Produces("application/json")]
public class ObjetivosController : ControllerBase
{
    private readonly ISender _sender;

    public ObjetivosController(ISender sender) => _sender = sender;

    /// <summary>Lista as caixinhas ativas, opcionalmente filtrando por status.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ObjetivoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] StatusObjetivo? status, CancellationToken ct)
        => Ok(await _sender.Send(new ListarObjetivosQuery(status), ct));

    /// <summary>
    /// Detalhes da caixinha. O ValorTotal vem da coluna persistida, sem somar
    /// as transações na hora da consulta.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ObjetivoDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
        => Ok(await _sender.Send(new ObterObjetivoPorIdQuery(id), ct));

    /// <summary>Cria uma nova caixinha.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ObjetivoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] CadastrarObjetivoCommand command, CancellationToken ct)
    {
        var objetivo = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = objetivo.Id }, objetivo);
    }

    /// <summary>Atualiza os dados da caixinha.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ObjetivoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        int id, [FromBody] AtualizarObjetivoCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command with { Id = id }, ct));

    /// <summary>
    /// Remoção lógica da caixinha. O registro continua no banco com
    /// EstaDeletado marcado e sai das listagens.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        await _sender.Send(new RemoverObjetivoCommand(id), ct);
        return NoContent();
    }

    /// <summary>Pausa a caixinha: ela deixa de aceitar transações.</summary>
    [HttpPatch("{id:int}/pausar")]
    [ProducesResponseType(typeof(ObjetivoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Pausar(int id, CancellationToken ct)
        => Ok(await _sender.Send(new AlterarStatusCommand(id, AcaoStatus.Pausar), ct));

    /// <summary>Retoma uma caixinha pausada.</summary>
    [HttpPatch("{id:int}/retomar")]
    [ProducesResponseType(typeof(ObjetivoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Retomar(int id, CancellationToken ct)
        => Ok(await _sender.Send(new AlterarStatusCommand(id, AcaoStatus.Retomar), ct));

    /// <summary>Cancela a caixinha definitivamente.</summary>
    [HttpPatch("{id:int}/cancelar")]
    [ProducesResponseType(typeof(ObjetivoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancelar(int id, CancellationToken ct)
        => Ok(await _sender.Send(new AlterarStatusCommand(id, AcaoStatus.Cancelar), ct));

    /// <summary>
    /// Envia a imagem de capa do objetivo (multipart/form-data).
    /// Aceita jpg, jpeg, png, gif, webp e bmp, com até 5MB.
    /// </summary>
    [HttpPost("{id:int}/capa")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [ProducesResponseType(typeof(ObjetivoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnviarCapa(int id, IFormFile arquivo, CancellationToken ct)
    {
        if (arquivo is null || arquivo.Length == 0)
        {
            return BadRequest(new RespostaErro
            {
                Titulo = "Arquivo não enviado",
                Status = StatusCodes.Status400BadRequest,
                Caminho = Request.Path,
                Erros = new List<string> { "Informe o campo 'arquivo' com a imagem da capa." }
            });
        }

        using var memoria = new MemoryStream();
        await arquivo.CopyToAsync(memoria, ct);

        var objetivo = await _sender.Send(
            new EnviarCapaCommand(id, memoria.ToArray(), arquivo.FileName), ct);

        return Ok(objetivo);
    }

    /// <summary>Devolve a imagem de capa do objetivo.</summary>
    [HttpGet("{id:int}/capa")]
    [Produces("image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterCapa(int id, CancellationToken ct)
    {
        var capa = await _sender.Send(new ObterCapaQuery(id), ct);
        return File(capa.Conteudo, capa.TipoConteudo);
    }

    /// <summary>
    /// Simula a evolução da caixinha com base em aportes mensais e
    /// rendimentos. Sem aporte informado, usa o aporte ideal cadastrado.
    /// </summary>
    [HttpPost("{id:int}/simular")]
    [ProducesResponseType(typeof(SimulacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Simular(
        int id, [FromBody] SimularEvolucaoQuery query, CancellationToken ct)
        => Ok(await _sender.Send(query with { IdObjetivo = id }, ct));
}
