using EduPlatform.Api.Data;
using EduPlatform.Api.DTOs;
using EduPlatform.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProgressoController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProgressoController(AppDbContext db) => _db = db;

    /// <summary>Marca uma aula como concluída por um usuário, registrando a nota obtida.</summary>
    [HttpPost("concluir-aula")]
    public async Task<ActionResult<UsuarioAulaConcluida>> ConcluirAula(ConcluirAulaDto dto)
    {
        if (dto.Nota is < 0 or > 10)
            return BadRequest("Nota deve estar entre 0 e 10.");

        if (!await _db.Usuarios.AnyAsync(u => u.Id == dto.IdUsuario))
            return BadRequest("Usuário não encontrado.");

        if (!await _db.Aulas.AnyAsync(a => a.Id == dto.IdAula))
            return BadRequest("Aula não encontrada.");

        var existente = await _db.UsuarioAulasConcluidas
            .FirstOrDefaultAsync(x => x.IdUsuario == dto.IdUsuario && x.IdAula == dto.IdAula);

        if (existente is not null)
        {
            // Já concluída anteriormente: atualiza nota e data.
            existente.Nota = dto.Nota;
            existente.DataConclusao = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(existente);
        }

        var conclusao = new UsuarioAulaConcluida
        {
            IdUsuario = dto.IdUsuario,
            IdAula = dto.IdAula,
            Nota = dto.Nota,
            DataConclusao = DateTime.UtcNow
        };

        _db.UsuarioAulasConcluidas.Add(conclusao);
        await _db.SaveChangesAsync();
        return Ok(conclusao);
    }

    /// <summary>
    /// Retorna o status de um curso para um usuário: tempo total, tempo concluído,
    /// tempo restante e percentual de progresso.
    /// </summary>
    [HttpGet("status-curso/{idCurso:guid}/usuario/{idUsuario:guid}")]
    public async Task<ActionResult<StatusCursoUsuarioDto>> StatusCurso(Guid idCurso, Guid idUsuario)
    {
        var curso = await _db.Cursos
            .Include(c => c.Modulos).ThenInclude(m => m.Aulas)
            .FirstOrDefaultAsync(c => c.Id == idCurso);

        if (curso is null) return NotFound("Curso não encontrado.");

        var todasAulas = curso.Modulos.SelectMany(m => m.Aulas).ToList();
        var idsAulas = todasAulas.Select(a => a.Id).ToHashSet();

        var concluidas = await _db.UsuarioAulasConcluidas
            .Where(x => x.IdUsuario == idUsuario && idsAulas.Contains(x.IdAula))
            .ToListAsync();

        var idsConcluidas = concluidas.Select(c => c.IdAula).ToHashSet();

        var tempoTotal = todasAulas.Sum(a => a.Duracao);
        var tempoConcluido = todasAulas.Where(a => idsConcluidas.Contains(a.Id)).Sum(a => a.Duracao);
        var tempoRestante = tempoTotal - tempoConcluido;
        var percentual = tempoTotal == 0 ? 0 : Math.Round((double)tempoConcluido / tempoTotal * 100, 2);

        var resultado = new StatusCursoUsuarioDto(
            curso.Id,
            curso.Nome,
            tempoTotal,
            tempoConcluido,
            tempoRestante,
            percentual,
            todasAulas.Count,
            idsConcluidas.Count);

        return Ok(resultado);
    }
}
