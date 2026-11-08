using Clinica.Api.Api.Seguranca;
using Clinica.Api.Aplicacao.InputModels;
using Clinica.Api.Aplicacao.Servicos;
using Clinica.Api.Aplicacao.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Api.Controllers;

/// <summary>CRUD dos serviços oferecidos pela clínica.</summary>
[ApiController]
[Route("api/servicos")]
[Authorize(Roles = Perfis.Todos)]
public class ServicosController : ControllerBase
{
    private readonly ServicoAppService _service;

    public ServicosController(ServicoAppService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(List<ServicoViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        => Ok(await _service.ListarAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ServicoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
        => Ok(await _service.ObterPorIdAsync(id, cancellationToken));

    [HttpPost]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(typeof(ServicoViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] ServicoInputModel entrada, CancellationToken cancellationToken)
    {
        var servico = await _service.CadastrarAsync(entrada, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = servico.Id }, servico);
    }

    /// <summary>
    /// Atualiza o serviço. Valor e duração novos valem só para agendamentos
    /// futuros: cada atendimento guarda a própria cópia.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(typeof(ServicoViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar(
        int id, [FromBody] ServicoInputModel entrada, CancellationToken cancellationToken)
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
