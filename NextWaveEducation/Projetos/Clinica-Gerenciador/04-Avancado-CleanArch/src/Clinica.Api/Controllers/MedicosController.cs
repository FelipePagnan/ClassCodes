using Clinica.Api.Seguranca;
using Clinica.Aplicacao.Dtos;
using Clinica.Aplicacao.Medicos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Controllers;

/// <summary>CRUD de médicos.</summary>
[ApiController]
[Route("api/medicos")]
[Authorize(Roles = Perfis.Todos)]
public class MedicosController : ControllerBase
{
    private readonly ISender _sender;

    public MedicosController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof(List<MedicoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? especialidade, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ListarMedicos(especialidade), cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MedicoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ObterMedico(id), cancellationToken));

    /// <summary>Busca pelo CRM. O número só é único dentro da UF.</summary>
    [HttpGet("crm/{uf}/{crm}")]
    [ProducesResponseType(typeof(MedicoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorCrm(
        string uf, string crm, CancellationToken cancellationToken)
    {
        var medico = await _sender.Send(new BuscarMedicoPorCrm(crm, uf), cancellationToken);

        return medico is null
            ? NotFound(new { mensagem = $"Nenhum médico com CRM/{uf.ToUpperInvariant()} {crm}." })
            : Ok(medico);
    }

    [HttpPost]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(typeof(MedicoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] CadastrarMedico comando, CancellationToken cancellationToken)
    {
        var medico = await _sender.Send(comando, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = medico.Id }, medico);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(typeof(MedicoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar(
        int id, [FromBody] AtualizarMedicoCorpo corpo, CancellationToken cancellationToken)
        => Ok(await _sender.Send(corpo.ParaComando(id), cancellationToken));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        await _sender.Send(new RemoverMedico(id), cancellationToken);
        return NoContent();
    }
}

/// <summary>Corpo do PUT. O Id vem da rota.</summary>
public record AtualizarMedicoCorpo(
    string NomeCompleto,
    string Crm,
    string UfCrm,
    string Especialidade,
    string? Email,
    string? Celular)
{
    public AtualizarMedico ParaComando(int id) => new(
        id, NomeCompleto, Crm, UfCrm, Especialidade, Email, Celular);
}
