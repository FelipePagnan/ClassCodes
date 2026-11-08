using Clinica.Api.Entidades;
using Clinica.Api.Modelos;
using Clinica.Api.Seguranca;
using Clinica.Api.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Controllers;

/// <summary>CRUD dos serviços oferecidos pela clínica.</summary>
[ApiController]
[Route("api/servicos")]
[Authorize(Roles = Perfis.Todos)]
public class ServicosController : ControllerBase
{
    private readonly ServicoService _service;

    public ServicosController(ServicoService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(List<Servico>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar() => Ok(await _service.ListarAsync());

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Servico), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterPorId(int id)
        => Ok(await _service.ObterPorIdAsync(id));

    [HttpPost]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(typeof(Servico), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar([FromBody] ServicoRequisicao requisicao)
    {
        var servico = await _service.CadastrarAsync(requisicao);
        return CreatedAtAction(nameof(ObterPorId), new { id = servico.Id }, servico);
    }

    /// <summary>
    /// Atualiza o serviço. Valor e duração novos valem só para agendamentos
    /// futuros: os atendimentos já marcados guardam a própria cópia.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(typeof(Servico), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] ServicoRequisicao requisicao)
        => Ok(await _service.AtualizarAsync(id, requisicao));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remover(int id)
    {
        await _service.RemoverAsync(id);
        return NoContent();
    }
}
