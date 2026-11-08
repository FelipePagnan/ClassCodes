using EduPlatform.Api.Data;
using EduPlatform.Api.DTOs;
using EduPlatform.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssinaturasController : ControllerBase
{
    private readonly AppDbContext _db;
    public AssinaturasController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Assinatura>>> GetAll()
        => Ok(await _db.Assinaturas.AsNoTracking().Include(a => a.Cursos).ToListAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Assinatura>> GetById(Guid id)
    {
        var assinatura = await _db.Assinaturas.AsNoTracking()
            .Include(a => a.Cursos)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (assinatura is null) return NotFound();
        return Ok(assinatura);
    }

    [HttpPost]
    public async Task<ActionResult<Assinatura>> Create(AssinaturaCreateDto dto)
    {
        var cursos = await _db.Cursos.Where(c => dto.IdsCursos.Contains(c.Id)).ToListAsync();

        var assinatura = new Assinatura
        {
            Nome = dto.Nome,
            Duracao = dto.Duracao,
            Valor = dto.Valor,
            Cursos = cursos
        };

        _db.Assinaturas.Add(assinatura);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = assinatura.Id }, assinatura);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, AssinaturaUpdateDto dto)
    {
        var assinatura = await _db.Assinaturas.Include(a => a.Cursos).FirstOrDefaultAsync(a => a.Id == id);
        if (assinatura is null) return NotFound();

        assinatura.Nome = dto.Nome;
        assinatura.Duracao = dto.Duracao;
        assinatura.Valor = dto.Valor;

        var cursos = await _db.Cursos.Where(c => dto.IdsCursos.Contains(c.Id)).ToListAsync();
        assinatura.Cursos = cursos;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var assinatura = await _db.Assinaturas.FindAsync(id);
        if (assinatura is null) return NotFound();
        _db.Assinaturas.Remove(assinatura);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Conecta (vincula) um usuário a uma assinatura, criando o registro UsuarioAssinatura.</summary>
    [HttpPost("conectar-usuario")]
    public async Task<ActionResult<UsuarioAssinatura>> ConectarUsuario(ConectarUsuarioAssinaturaDto dto)
    {
        var usuario = await _db.Usuarios.FindAsync(dto.IdUsuario);
        if (usuario is null) return BadRequest("Usuário não encontrado.");

        var assinatura = await _db.Assinaturas.FindAsync(dto.IdAssinatura);
        if (assinatura is null) return BadRequest("Assinatura não encontrada.");

        var dataInicio = DateTime.UtcNow;
        var vinculo = new UsuarioAssinatura
        {
            IdUsuario = dto.IdUsuario,
            IdAssinatura = dto.IdAssinatura,
            Status = StatusUsuarioAssinatura.Pendente,
            DataInicio = dataInicio,
            DataExpiracao = dataInicio.AddDays(assinatura.Duracao)
        };

        _db.UsuarioAssinaturas.Add(vinculo);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = assinatura.Id }, vinculo);
    }
}
