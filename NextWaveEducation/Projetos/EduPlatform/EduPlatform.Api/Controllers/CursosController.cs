using EduPlatform.Api.Data;
using EduPlatform.Api.DTOs;
using EduPlatform.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CursosController : ControllerBase
{
    private readonly AppDbContext _db;
    public CursosController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Curso>>> GetAll()
        => Ok(await _db.Cursos.AsNoTracking().Include(c => c.Modulos).ToListAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Curso>> GetById(Guid id)
    {
        var curso = await _db.Cursos.AsNoTracking()
            .Include(c => c.Modulos).ThenInclude(m => m.Aulas)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (curso is null) return NotFound();
        return Ok(curso);
    }

    [HttpPost]
    public async Task<ActionResult<Curso>> Create(CursoCreateDto dto)
    {
        var curso = new Curso { Nome = dto.Nome, Descricao = dto.Descricao, Cover = dto.Cover };
        _db.Cursos.Add(curso);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = curso.Id }, curso);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, CursoUpdateDto dto)
    {
        var curso = await _db.Cursos.FindAsync(id);
        if (curso is null) return NotFound();

        curso.Nome = dto.Nome;
        curso.Descricao = dto.Descricao;
        curso.Cover = dto.Cover;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var curso = await _db.Cursos.FindAsync(id);
        if (curso is null) return NotFound();
        _db.Cursos.Remove(curso);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
public class ModulosController : ControllerBase
{
    private readonly AppDbContext _db;
    public ModulosController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Modulo>>> GetAll([FromQuery] Guid? idCurso)
    {
        var query = _db.Modulos.AsNoTracking().AsQueryable();
        if (idCurso.HasValue) query = query.Where(m => m.IdCurso == idCurso.Value);
        return Ok(await query.Include(m => m.Aulas).ToListAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Modulo>> GetById(Guid id)
    {
        var modulo = await _db.Modulos.AsNoTracking().Include(m => m.Aulas).FirstOrDefaultAsync(m => m.Id == id);
        if (modulo is null) return NotFound();
        return Ok(modulo);
    }

    [HttpPost]
    public async Task<ActionResult<Modulo>> Create(ModuloCreateDto dto)
    {
        if (!await _db.Cursos.AnyAsync(c => c.Id == dto.IdCurso))
            return BadRequest("Curso informado não existe.");

        var modulo = new Modulo { Nome = dto.Nome, Descricao = dto.Descricao, IdCurso = dto.IdCurso };
        _db.Modulos.Add(modulo);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = modulo.Id }, modulo);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, ModuloUpdateDto dto)
    {
        var modulo = await _db.Modulos.FindAsync(id);
        if (modulo is null) return NotFound();

        modulo.Nome = dto.Nome;
        modulo.Descricao = dto.Descricao;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var modulo = await _db.Modulos.FindAsync(id);
        if (modulo is null) return NotFound();
        _db.Modulos.Remove(modulo);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
public class AulasController : ControllerBase
{
    private readonly AppDbContext _db;
    public AulasController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Aula>>> GetAll([FromQuery] Guid? idModulo)
    {
        var query = _db.Aulas.AsNoTracking().AsQueryable();
        if (idModulo.HasValue) query = query.Where(a => a.IdModulo == idModulo.Value);
        return Ok(await query.ToListAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Aula>> GetById(Guid id)
    {
        var aula = await _db.Aulas.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        if (aula is null) return NotFound();
        return Ok(aula);
    }

    [HttpPost]
    public async Task<ActionResult<Aula>> Create(AulaCreateDto dto)
    {
        if (!await _db.Modulos.AnyAsync(m => m.Id == dto.IdModulo))
            return BadRequest("Módulo informado não existe.");

        var aula = new Aula
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            LinkVideo = dto.LinkVideo,
            Duracao = dto.Duracao,
            IdModulo = dto.IdModulo
        };
        _db.Aulas.Add(aula);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = aula.Id }, aula);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, AulaUpdateDto dto)
    {
        var aula = await _db.Aulas.FindAsync(id);
        if (aula is null) return NotFound();

        aula.Nome = dto.Nome;
        aula.Descricao = dto.Descricao;
        aula.LinkVideo = dto.LinkVideo;
        aula.Duracao = dto.Duracao;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var aula = await _db.Aulas.FindAsync(id);
        if (aula is null) return NotFound();
        _db.Aulas.Remove(aula);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
