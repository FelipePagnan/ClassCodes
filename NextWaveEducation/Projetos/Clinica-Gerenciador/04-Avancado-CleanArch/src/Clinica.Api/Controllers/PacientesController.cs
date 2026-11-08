using Clinica.Api.Seguranca;
using Clinica.Aplicacao.Anexos;
using Clinica.Aplicacao.Dtos;
using Clinica.Aplicacao.Pacientes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Controllers;

/// <summary>
/// CRUD de pacientes, com busca por CPF e por celular.
///
/// Os controllers do nível Avançado são finos de propósito: recebem o
/// comando ou a consulta, mandam pelo MediatR e devolvem. Validação,
/// regra e transação acontecem no pipeline e nos handlers.
/// </summary>
[ApiController]
[Route("api/pacientes")]
[Authorize(Roles = Perfis.Todos)]
public class PacientesController : ControllerBase
{
    private readonly ISender _sender;

    public PacientesController(ISender sender) => _sender = sender;

    /// <summary>Lista, com busca livre opcional por nome, CPF ou celular.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PacienteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new PesquisarPacientes(busca), cancellationToken));

    /// <summary>Busca pelo CPF. Aceita formatado ou só dígitos.</summary>
    [HttpGet("cpf/{cpf}")]
    [ProducesResponseType(typeof(PacienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorCpf(string cpf, CancellationToken cancellationToken)
    {
        var paciente = await _sender.Send(new BuscarPacientePorCpf(cpf), cancellationToken);

        return paciente is null
            ? NotFound(new { mensagem = $"Nenhum paciente com o CPF {cpf}." })
            : Ok(paciente);
    }

    /// <summary>
    /// Busca pelo celular. Devolve lista: o mesmo número costuma atender mais
    /// de um paciente da família.
    /// </summary>
    [HttpGet("celular/{celular}")]
    [ProducesResponseType(typeof(List<PacienteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BuscarPorCelular(
        string celular, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new BuscarPacientesPorCelular(celular), cancellationToken));

    /// <summary>Ficha do paciente com o histórico de atendimentos.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FichaPacienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ObterFichaDoPaciente(id), cancellationToken));

    /// <summary>Todos os anexos clínicos do paciente, de todos os atendimentos.</summary>
    [HttpGet("{id:int}/anexos")]
    [ProducesResponseType(typeof(List<AnexoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarAnexos(int id, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ListarAnexosDoPaciente(id), cancellationToken));

    [HttpPost]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(PacienteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] CadastrarPaciente comando, CancellationToken cancellationToken)
    {
        var paciente = await _sender.Send(comando, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = paciente.Id }, paciente);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(typeof(PacienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar(
        int id, [FromBody] AtualizarPacienteCorpo corpo, CancellationToken cancellationToken)
        => Ok(await _sender.Send(corpo.ParaComando(id), cancellationToken));

    /// <summary>Remoção lógica: o histórico clínico é preservado.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Perfis.AdministradorOuRecepcionista)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Remover(int id, CancellationToken cancellationToken)
    {
        await _sender.Send(new RemoverPaciente(id), cancellationToken);
        return NoContent();
    }
}

/// <summary>
/// Corpo do PUT. O Id vem da rota, então não entra no corpo — assim não há
/// como mandar um Id na URL e outro no JSON.
/// </summary>
public record AtualizarPacienteCorpo(
    string NomeCompleto,
    string Cpf,
    string Celular,
    string? Email,
    DateTime DataNascimento,
    Clinica.Dominio.Enums.Sexo Sexo,
    string? Observacoes)
{
    public AtualizarPaciente ParaComando(int id) => new(
        id, NomeCompleto, Cpf, Celular, Email, DataNascimento, Sexo, Observacoes);
}
