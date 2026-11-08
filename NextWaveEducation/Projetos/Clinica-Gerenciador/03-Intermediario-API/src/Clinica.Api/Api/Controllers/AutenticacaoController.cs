using Clinica.Api.Api.Seguranca;
using Clinica.Api.Aplicacao.InputModels;
using Clinica.Api.Aplicacao.Servicos;
using Clinica.Api.Aplicacao.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api.Api.Controllers;

/// <summary>Login e dados do usuário autenticado.</summary>
[ApiController]
[Route("api/autenticacao")]
public class AutenticacaoController : ControllerBase
{
    private readonly UsuarioAppService _service;

    public AutenticacaoController(UsuarioAppService service) => _service = service;

    /// <summary>
    /// Autentica e devolve o token JWT. O token vai no cabeçalho
    /// Authorization: Bearer {token} nas demais chamadas.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginInputModel entrada, CancellationToken cancellationToken)
        => Ok(await _service.LoginAsync(entrada, cancellationToken));

    /// <summary>Usuário autenticado. Serve para conferir o token.</summary>
    [HttpGet("eu")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Eu() => Ok(new
    {
        id = User.ObterUsuarioId(),
        nome = User.Identity?.Name,
        email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
        perfil = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value,
        medicoId = User.ObterMedicoId()
    });

    /// <summary>Troca a senha do usuário autenticado.</summary>
    [HttpPut("minha-senha")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TrocarMinhaSenha(
        [FromBody] TrocaDeSenhaInputModel entrada, CancellationToken cancellationToken)
    {
        await _service.AlterarSenhaAsync(User.ObterUsuarioId()!.Value, entrada, cancellationToken);
        return NoContent();
    }
}
