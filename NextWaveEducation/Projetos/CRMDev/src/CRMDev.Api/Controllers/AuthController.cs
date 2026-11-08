using CRMDev.Application.DTOs;
using CRMDev.Application.Interfaces;
using CRMDev.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CRMDev.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>Cria um novo usuário (vendedor/administrador) do CRMDev.</summary>
    [HttpPost("registrar")]
    public async Task<ActionResult<AuthResponseDto>> Registrar(RegistrarUsuarioDto dto)
    {
        var usuarioExistente = await _userManager.FindByEmailAsync(dto.Email);
        if (usuarioExistente is not null)
        {
            return Conflict(new { mensagem = "Já existe um usuário cadastrado com este e-mail." });
        }

        var usuario = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            Nome = dto.Nome
        };

        var resultado = await _userManager.CreateAsync(usuario, dto.Senha);
        if (!resultado.Succeeded)
        {
            return BadRequest(new { erros = resultado.Errors.Select(e => e.Description) });
        }

        var (token, expiraEm) = _jwtTokenService.GerarToken(usuario.Id, usuario.Nome, usuario.Email!);
        return Ok(new AuthResponseDto(token, expiraEm, usuario.Nome, usuario.Email!));
    }

    /// <summary>Autentica um usuário e retorna um token JWT.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var usuario = await _userManager.FindByEmailAsync(dto.Email);
        if (usuario is null)
        {
            return Unauthorized(new { mensagem = "E-mail ou senha inválidos." });
        }

        var senhaValida = await _userManager.CheckPasswordAsync(usuario, dto.Senha);
        if (!senhaValida)
        {
            return Unauthorized(new { mensagem = "E-mail ou senha inválidos." });
        }

        var (token, expiraEm) = _jwtTokenService.GerarToken(usuario.Id, usuario.Nome, usuario.Email!);
        return Ok(new AuthResponseDto(token, expiraEm, usuario.Nome, usuario.Email!));
    }
}
