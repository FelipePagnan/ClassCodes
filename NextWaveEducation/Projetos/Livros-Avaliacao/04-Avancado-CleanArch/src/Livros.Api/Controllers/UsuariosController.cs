using Livros.Api.Middlewares;
using Livros.Application.Common;
using Livros.Application.Usuarios.Commands.AtualizarUsuario;
using Livros.Application.Usuarios.Commands.CadastrarUsuario;
using Livros.Application.Usuarios.Commands.RemoverUsuario;
using Livros.Application.Usuarios.Queries.ListarUsuarios;
using Livros.Application.Usuarios.Queries.ObterUsuarioPorId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Livros.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Produces("application/json")]
public class UsuariosController : ControllerBase
{
    private readonly ISender _sender;

    public UsuariosController(ISender sender) => _sender = sender;

    /// <summary>Lista os usuários cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<UsuarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken ct)
        => Ok(await _sender.Send(new ListarUsuariosQuery(), ct));

    /// <summary>Detalhes do usuário com as avaliações feitas por ele.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UsuarioDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id, CancellationToken ct)
        => Ok(await _sender.Send(new ObterUsuarioPorIdQuery(id), ct));

    /// <summary>Cadastra um novo usuário.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar(
        [FromBody] CadastrarUsuarioCommand command, CancellationToken ct)
    {
        var usuario = await _sender.Send(command, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = usuario.Id }, usuario);
    }

    /// <summary>Atualiza os dados de um usuário.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(
        int id, [FromBody] AtualizarUsuarioCommand command, CancellationToken ct)
        => Ok(await _sender.Send(command with { Id = id }, ct));

    /// <summary>Remove um usuário que ainda não possui avaliações.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        await _sender.Send(new RemoverUsuarioCommand(id), ct);
        return NoContent();
    }
}
