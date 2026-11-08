using Clinica.Api.Api.Seguranca;
using Clinica.Api.Aplicacao.InputModels;
using Clinica.Api.Aplicacao.Servicos;
using Clinica.Api.Aplicacao.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Api.Controllers;

/// <summary>Usuários do sistema. Só o administrador gerencia.</summary>
[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = Perfis.Administrador)]
public class UsuariosController : ControllerBase
{
    private readonly UsuarioAppService _service;

    public UsuariosController(UsuarioAppService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(List<UsuarioViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        => Ok(await _service.ListarAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UsuarioViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
        => Ok(await _service.ObterPorIdAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] UsuarioInputModel entrada, CancellationToken cancellationToken)
    {
        var usuario = await _service.CadastrarAsync(entrada, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = usuario.Id }, usuario);
    }

    [HttpPost("{id:int}/desativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Desativar(int id, CancellationToken cancellationToken)
    {
        await _service.DesativarAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/reativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reativar(int id, CancellationToken cancellationToken)
    {
        await _service.ReativarAsync(id, cancellationToken);
        return NoContent();
    }
}
