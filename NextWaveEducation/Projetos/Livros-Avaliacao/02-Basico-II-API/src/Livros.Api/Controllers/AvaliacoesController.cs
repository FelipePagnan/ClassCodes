using Livros.Api.Dados;
using Livros.Api.Dtos;
using Livros.Api.Entidades;
using Livros.Api.Excecoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Livros.Api.Controllers;

/// <summary>
/// Cadastro e listagem de avaliações de um livro.
/// </summary>
[ApiController]
[Route("api/livros/{idLivro:int}/avaliacoes")]
public class AvaliacoesController : ControllerBase
{
    private readonly LivrosContext _context;

    public AvaliacoesController(LivrosContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AvaliacaoDto>>> Listar(int idLivro)
    {
        if (!await _context.Livros.AnyAsync(l => l.Id == idLivro))
            throw new NaoEncontradoException($"Livro com id {idLivro} não encontrado.");

        var avaliacoes = await _context.Avaliacoes
            .Include(a => a.Usuario)
            .Where(a => a.IdLivro == idLivro)
            .OrderByDescending(a => a.DataCriacao)
            .ToListAsync();

        return Ok(avaliacoes.Select(a => new AvaliacaoDto(
            a.Id, a.Nota, a.Descricao, a.IdUsuario, a.Usuario?.Nome,
            a.DataInicioLeitura, a.DataFimLeitura, a.DiasDeLeitura, a.DataCriacao)));
    }

    [HttpPost]
    public async Task<ActionResult<AvaliacaoDto>> Cadastrar(int idLivro, CriarAvaliacaoDto dto)
    {
        var livro = await _context.Livros
            .Include(l => l.Avaliacoes)
            .FirstOrDefaultAsync(l => l.Id == idLivro);

        if (livro is null)
            throw new NaoEncontradoException($"Livro com id {idLivro} não encontrado.");

        var usuario = await _context.Usuarios.FindAsync(dto.IdUsuario);
        if (usuario is null)
            throw new NaoEncontradoException($"Usuário com id {dto.IdUsuario} não encontrado.");

        // Regra de negócio: a nota deve ser de 1 a 5.
        if (dto.Nota < Avaliacao.NotaMinima || dto.Nota > Avaliacao.NotaMaxima)
            throw new RegraNegocioException(
                $"A nota deve ser de {Avaliacao.NotaMinima} a {Avaliacao.NotaMaxima}. Valor informado: {dto.Nota}.");

        // Regra de negócio: a data de início não pode ser maior que a de fim de leitura.
        if (dto.DataInicioLeitura.Date > dto.DataFimLeitura.Date)
            throw new RegraNegocioException(
                $"A data de início da leitura ({dto.DataInicioLeitura:dd/MM/yyyy}) não pode ser " +
                $"maior que a data de fim ({dto.DataFimLeitura:dd/MM/yyyy}).");

        if (dto.DataFimLeitura.Date > DateTime.Today)
            throw new RegraNegocioException("A data de fim da leitura não pode estar no futuro.");

        if (livro.Avaliacoes.Any(a => a.IdUsuario == dto.IdUsuario))
            throw new RegraNegocioException($"'{usuario.Nome}' já avaliou o livro '{livro.Titulo}'.");

        var avaliacao = new Avaliacao
        {
            IdLivro = livro.Id,
            IdUsuario = usuario.Id,
            Nota = dto.Nota,
            Descricao = dto.Descricao?.Trim() ?? string.Empty,
            DataInicioLeitura = dto.DataInicioLeitura.Date,
            DataFimLeitura = dto.DataFimLeitura.Date,
            DataCriacao = DateTime.Now
        };

        _context.Avaliacoes.Add(avaliacao);

        // A cada avaliação cadastrada a nota média do livro é recalculada.
        livro.Avaliacoes.Add(avaliacao);
        livro.RecalcularNotaMedia();

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Listar), new { idLivro = livro.Id }, new AvaliacaoDto(
            avaliacao.Id, avaliacao.Nota, avaliacao.Descricao, avaliacao.IdUsuario, usuario.Nome,
            avaliacao.DataInicioLeitura, avaliacao.DataFimLeitura, avaliacao.DiasDeLeitura,
            avaliacao.DataCriacao));
    }
}
