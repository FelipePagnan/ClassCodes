using Clinica.Api.Entidades;
using Clinica.Api.Modelos;
using Clinica.Api.Seguranca;
using Clinica.Api.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Controllers;

/// <summary>CRUD de pacientes, com busca por CPF e por celular.</summary>
[ApiController]
[Route("api/pacientes")]
[Authorize(Roles = Perfis.Todos)]
public class PacientesController : ControllerBase
{
    private readonly PacienteService _service;

    public PacientesController(PacienteService service) => _service = service;

    /// <summary>Lista os pacientes, com busca livre opcional por nome, CPF ou celular.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<Paciente>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? busca)
        => Ok(await _service.PesquisarAsync(busca));

    /// <summary>Busca o paciente pelo CPF. Aceita formatado ou só dígitos.</summary>
    [HttpGet("cpf/{cpf}")]
    [ProducesResponseType(typeof(Paciente), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorCpf(string cpf)
    {
        var paciente = await _service.BuscarPorCpfAsync(cpf);

        return paciente is null
            ? NotFound(new { mensagem = $"Nenhum paciente com o CPF {cpf}." })
            : Ok(paciente);
    }

    /// <summary>
    /// Busca pacientes pelo celular. Devolve lista porque o mesmo número
    /// costuma atender mais de um paciente da família.
    /// </summary>
    [HttpGet("celular/{celular}")]
    [ProducesResponseType(typeof(List<Paciente>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuscarPorCelular(string celular)
        => Ok(await _service.BuscarPorCelularAsync(celular));

    /// <summary>Ficha do paciente com o histórico de atendimentos e anexos.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Paciente), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterPorId(int id)
        => Ok(await _service.ObterFichaAsync(id));

    [HttpPost]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(Paciente), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar([FromBody] PacienteRequisicao requisicao)
    {
        var paciente = await _service.CadastrarAsync(requisicao);
        return CreatedAtAction(nameof(ObterPorId), new { id = paciente.Id }, paciente);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(Paciente), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] PacienteRequisicao requisicao)
        => Ok(await _service.AtualizarAsync(id, requisicao));

    /// <summary>Remoção lógica: o histórico clínico é preservado.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remover(int id)
    {
        await _service.RemoverAsync(id);
        return NoContent();
    }
}
