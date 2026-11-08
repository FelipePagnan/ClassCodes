using Clinica.Api.Seguranca;
using Clinica.Aplicacao.Dtos;
using Clinica.Aplicacao.Servicos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Controllers;

/// <summary>CRUD dos serviços oferecidos pela clínica.</summary>
[ApiController]
[Route("api/servicos")]
[Authorize(Roles = Perfis.Todos)]
public class ServicosController : ControllerBase
{
    private readonly ISender _sender;

    public ServicosController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof(List<ServicoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ListarServicos(), cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ServicoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ObterServico(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(typeof(ServicoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] CadastrarServico comando, CancellationToken cancellationToken)
    {
        var servico = await _sender.Send(comando, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = servico.Id }, servico);
    }

    /// <summary>
    /// Atualiza o serviço. Valor e duração novos valem só para agendamentos
    /// futuros: cada atendimento guarda a própria cópia.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(typeof(ServicoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar(
        int id, [FromBody] AtualizarServicoCorpo corpo, CancellationToken cancellationToken)
        => Ok(await _sender.Send(corpo.ParaComando(id), cancellationToken));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        await _sender.Send(new RemoverServico(id), cancellationToken);
        return NoContent();
    }
}

/// <summary>Corpo do PUT. O Id vem da rota.</summary>
public record AtualizarServicoCorpo(
    string Nome,
    string? Descricao,
    decimal Valor,
    int DuracaoMinutos)
{
    public AtualizarServico ParaComando(int id) => new(
        id, Nome, Descricao, Valor, DuracaoMinutos);
}
