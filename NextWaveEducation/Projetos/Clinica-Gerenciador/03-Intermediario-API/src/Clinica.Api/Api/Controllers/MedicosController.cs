using Clinica.Api.Api.Seguranca;
using Clinica.Api.Aplicacao.InputModels;
using Clinica.Api.Aplicacao.Servicos;
using Clinica.Api.Aplicacao.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Api.Controllers;

/// <summary>CRUD de médicos.</summary>
[ApiController]
[Route("api/medicos")]
[Authorize(Roles = Perfis.Todos)]
public class MedicosController : ControllerBase
{
    private readonly MedicoAppService _service;

    public MedicosController(MedicoAppService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(List<MedicoViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? especialidade, CancellationToken cancellationToken)
        => Ok(await _service.ListarAsync(especialidade, cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MedicoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
        => Ok(await _service.ObterPorIdAsync(id, cancellationToken));

    /// <summary>Busca pelo CRM. O número só é único dentro da UF.</summary>
    [HttpGet("crm/{uf}/{crm}")]
    [ProducesResponseType(typeof(MedicoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorCrm(
        string uf, string crm, CancellationToken cancellationToken)
    {
        var medico = await _service.BuscarPorCrmAsync(crm, uf, cancellationToken);

        return medico is null
            ? NotFound(new { mensagem = $"Nenhum médico com CRM/{uf.ToUpperInvariant()} {crm}." })
            : Ok(medico);
    }

    [HttpPost]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(typeof(MedicoViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] MedicoInputModel entrada, CancellationToken cancellationToken)
    {
        var medico = await _service.CadastrarAsync(entrada, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = medico.Id }, medico);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(typeof(MedicoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar(
        int id, [FromBody] MedicoInputModel entrada, CancellationToken cancellationToken)
        => Ok(await _service.AtualizarAsync(id, entrada, cancellationToken));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        await _service.RemoverAsync(id, cancellationToken);
        return NoContent();
    }
}
