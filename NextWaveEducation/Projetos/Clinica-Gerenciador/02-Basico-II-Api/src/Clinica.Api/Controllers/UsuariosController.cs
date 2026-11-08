using Clinica.Api.Modelos;
using Clinica.Api.Seguranca;
using Clinica.Api.Servicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Controllers;

/// <summary>
/// Usuários do sistema e os três perfis de acesso. Só o administrador
/// gerencia usuários; qualquer um pode ver quem está logado e trocar a
/// própria senha.
/// </summary>
[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly UsuarioService _service;

    public UsuariosController(UsuarioService service) => _service = service;

    /// <summary>
    /// Devolve o usuário autenticado. Serve para testar as credenciais sem
    /// precisar acertar nenhuma outra rota.
    /// </summary>
    [HttpGet("eu")]
    [Authorize(Roles = Perfis.Todos)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Eu() => Ok(new
    {
        nome = User.Identity?.Name,
        email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
        perfil = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
        medicoId = User.ObterMedicoId()
    });

    [HttpGet]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar()
    {
        var usuarios = await _service.ListarAsync();

        // O hash e o salt nunca saem da aplicação.
        return Ok(usuarios.Select(u => new
        {
            u.Id,
            u.Nome,
            u.Email,
            perfil = u.Perfil.ToString(),
            u.MedicoId,
            medico = u.Medico?.NomeCompleto,
            u.Ativo,
            u.DataCriacao,
            u.UltimoAcesso
        }));
    }

    [HttpPost]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar([FromBody] UsuarioRequisicao requisicao)
    {
        var usuario = await _service.CadastrarAsync(requisicao);

        return Created($"/api/usuarios/{usuario.Id}", new
        {
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            perfil = usuario.Perfil.ToString(),
            usuario.MedicoId
        });
    }

    /// <summary>Troca a senha do usuário autenticado.</summary>
    [HttpPut("minha-senha")]
    [Authorize(Roles = Perfis.Todos)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TrocarMinhaSenha([FromBody] TrocaDeSenhaRequisicao requisicao)
    {
        var id = int.Parse(
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        await _service.AlterarSenhaAsync(id, requisicao);
        return NoContent();
    }

    [HttpPost("{id:int}/desativar")]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Desativar(int id)
    {
        await _service.DesativarAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/reativar")]
    [Authorize(Roles = Perfis.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reativar(int id)
    {
        await _service.ReativarAsync(id);
        return NoContent();
    }
}
