using Clinica.Api.Api.Seguranca;
using Clinica.Api.Aplicacao.InputModels;
using Clinica.Api.Aplicacao.Servicos;
using Clinica.Api.Aplicacao.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Api.Controllers;

/// <summary>CRUD de pacientes, com busca por CPF e por celular.</summary>
[ApiController]
[Route("api/pacientes")]
[Authorize(Roles = Perfis.Todos)]
public class PacientesController : ControllerBase
{
    private readonly PacienteAppService _service;
    private readonly AtendimentoAppService _atendimentoService;

    public PacientesController(
        PacienteAppService service, AtendimentoAppService atendimentoService)
    {
        _service = service;
        _atendimentoService = atendimentoService;
    }

    /// <summary>Lista, com busca livre opcional por nome, CPF ou celular.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PacienteViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca, CancellationToken cancellationToken)
        => Ok(await _service.PesquisarAsync(busca, cancellationToken));

    /// <summary>Busca pelo CPF. Aceita formatado ou só dígitos.</summary>
    [HttpGet("cpf/{cpf}")]
    [ProducesResponseType(typeof(PacienteViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorCpf(string cpf, CancellationToken cancellationToken)
    {
        var paciente = await _service.BuscarPorCpfAsync(cpf, cancellationToken);

        return paciente is null
            ? NotFound(new { mensagem = $"Nenhum paciente com o CPF {cpf}." })
            : Ok(paciente);
    }

    /// <summary>
    /// Busca pelo celular. Devolve lista: o mesmo número costuma atender mais
    /// de um paciente da família.
    /// </summary>
    [HttpGet("celular/{celular}")]
    [ProducesResponseType(typeof(List<PacienteViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BuscarPorCelular(
        string celular, CancellationToken cancellationToken)
        => Ok(await _service.BuscarPorCelularAsync(celular, cancellationToken));

    /// <summary>Ficha do paciente com o histórico de atendimentos.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FichaPacienteViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
        => Ok(await _service.ObterFichaAsync(id, cancellationToken));

    /// <summary>Todos os anexos clínicos do paciente, de todos os atendimentos.</summary>
    [HttpGet("{id:int}/anexos")]
    [ProducesResponseType(typeof(List<AnexoViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarAnexos(int id, CancellationToken cancellationToken)
        => Ok(await _atendimentoService.ListarAnexosDoPacienteAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(PacienteViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] PacienteInputModel entrada, CancellationToken cancellationToken)
    {
        var paciente = await _service.CadastrarAsync(entrada, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = paciente.Id }, paciente);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(PacienteViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar(
        int id, [FromBody] PacienteInputModel entrada, CancellationToken cancellationToken)
        => Ok(await _service.AtualizarAsync(id, entrada, cancellationToken));

    /// <summary>Remoção lógica: o histórico clínico é preservado.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        await _service.RemoverAsync(id, cancellationToken);
        return NoContent();
    }
}
