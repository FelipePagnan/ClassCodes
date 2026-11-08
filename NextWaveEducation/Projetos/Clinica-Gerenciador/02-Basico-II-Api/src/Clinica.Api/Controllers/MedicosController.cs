using Clinica.Api.Entidades;
using Clinica.Api.Modelos;
using Clinica.Api.Seguranca;
using Clinica.Api.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Controllers;

/// <summary>CRUD de médicos.</summary>
[ApiController]
[Route("api/medicos")]
[Authorize(Roles = Perfis.Todos)]
public class MedicosController : ControllerBase
{
    private readonly MedicoService _service;

    public MedicosController(MedicoService service) => _service = service;

    /// <summary>Lista os médicos, podendo filtrar por especialidade.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<Medico>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? especialidade)
        => Ok(await _service.ListarAsync(especialidade));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Medico), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterPorId(int id)
        => Ok(await _service.ObterPorIdAsync(id));

    /// <summary>Busca pelo CRM. O número só é único dentro da UF.</summary>
    [HttpGet("crm/{uf}/{crm}")]
    [ProducesResponseType(typeof(Medico), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorCrm(string uf, string crm)
    {
        var medico = await _service.BuscarPorCrmAsync(crm, uf);

        return medico is null
            ? NotFound(new { mensagem = $"Nenhum médico com CRM/{uf.ToUpperInvariant()} {crm}." })
            : Ok(medico);
    }

    [HttpPost]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(typeof(Medico), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar([FromBody] MedicoRequisicao requisicao)
    {
        var medico = await _service.CadastrarAsync(requisicao);
        return CreatedAtAction(nameof(ObterPorId), new { id = medico.Id }, medico);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(typeof(Medico), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] MedicoRequisicao requisicao)
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
