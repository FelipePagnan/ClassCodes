using Clinica.Api.Seguranca;
using Clinica.Aplicacao.Contratos;
using Clinica.Aplicacao.Dtos;
using Clinica.Aplicacao.Usuarios;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Controllers;

/// <summary>Login e dados do usuário autenticado.</summary>
[ApiController]
[Route("api/autenticacao")]
public class AutenticacaoController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IUsuarioAtual _usuarioAtual;

    public AutenticacaoController(ISender sender, IUsuarioAtual usuarioAtual)
    {
        _sender = sender;
        _usuarioAtual = usuarioAtual;
    }

    /// <summary>
    /// Autentica e devolve o token JWT. O token vai no cabeçalho
    /// Authorization: Bearer {token} nas demais chamadas.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] AutenticarUsuario comando, CancellationToken cancellationToken)
        => Ok(await _sender.Send(comando, cancellationToken));

    /// <summary>Usuário autenticado. Serve para conferir o token.</summary>
    [HttpGet("eu")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Eu() => Ok(new
    {
        id = _usuarioAtual.UsuarioId,
        nome = _usuarioAtual.Nome,
        perfil = _usuarioAtual.Perfil,
        medicoId = _usuarioAtual.MedicoId
    });

    /// <summary>Troca a senha do usuário autenticado.</summary>
    [HttpPut("minha-senha")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TrocarMinhaSenha(
        [FromBody] TrocaDeSenhaRequisicao corpo, CancellationToken cancellationToken)
    {
        // O Id vem do token, nunca do corpo: senão qualquer um trocaria a
        // senha de qualquer um.
        await _sender.Send(
            new AlterarSenha(_usuarioAtual.UsuarioId!.Value, corpo.SenhaAtual, corpo.SenhaNova),
            cancellationToken);

        return NoContent();
    }
}

/// <summary>Corpo da troca de senha. O Id do usuário vem do token.</summary>
public record TrocaDeSenhaRequisicao(string SenhaAtual, string SenhaNova);
