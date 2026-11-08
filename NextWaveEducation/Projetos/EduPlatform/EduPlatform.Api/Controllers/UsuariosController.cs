using EduPlatform.Api.Data;
using EduPlatform.Api.DTOs;
using EduPlatform.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsuariosController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> GetAll()
    {
        var usuarios = await _db.Usuarios.AsNoTracking().ToListAsync();
        return Ok(usuarios.Select(UsuarioResponseDto.FromEntity));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UsuarioResponseDto>> GetById(Guid id)
    {
        var usuario = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (usuario is null) return NotFound();
        return Ok(UsuarioResponseDto.FromEntity(usuario));
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioResponseDto>> Create(UsuarioCreateDto dto)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email))
            return Conflict("Já existe um usuário com este e-mail.");

        var usuario = new Usuario
        {
            Nome = dto.Nome,
            Sobrenome = dto.Sobrenome,
            Email = dto.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            DataNascimento = dto.DataNascimento,
            Documento = dto.Documento,
            TelefoneCelular = dto.TelefoneCelular,
            Role = dto.Role,
            Ativo = true
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, UsuarioResponseDto.FromEntity(usuario));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UsuarioUpdateDto dto)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();

        usuario.Nome = dto.Nome;
        usuario.Sobrenome = dto.Sobrenome;
        usuario.Email = dto.Email;
        usuario.DataNascimento = dto.DataNascimento;
        usuario.Documento = dto.Documento;
        usuario.TelefoneCelular = dto.TelefoneCelular;
        usuario.Role = dto.Role;
        usuario.Ativo = dto.Ativo;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();

        // Soft delete: apenas inativa, preservando histórico de aulas/assinaturas.
        usuario.Ativo = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
