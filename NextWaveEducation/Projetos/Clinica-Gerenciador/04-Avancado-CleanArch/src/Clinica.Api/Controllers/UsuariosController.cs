using Clinica.Api.Seguranca;
using Clinica.Aplicacao.Dtos;
using Clinica.Aplicacao.Usuarios;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Controllers;

/// <summary>Usuários do sistema. Só o administrador gerencia.</summary>
[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = Perfis.Administrador)]
public class UsuariosController : ControllerBase
{
    private readonly ISender _sender;

    public UsuariosController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof(List<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ListarUsuarios(), cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken cancellationToken)
        => Ok(await _sender.Send(new ObterUsuario(id), cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] CadastrarUsuario comando, CancellationToken cancellationToken)
    {
        var usuario = await _sender.Send(comando, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = usuario.Id }, usuario);
    }

    [HttpPost("{id:int}/desativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Desativar(int id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DesativarUsuario(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/reativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reativar(int id, CancellationToken cancellationToken)
    {
        await _sender.Send(new ReativarUsuario(id), cancellationToken);
        return NoContent();
    }
}
