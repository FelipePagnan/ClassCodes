using Livros.Api.Dados;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Livros.Api.Controllers;

[ApiController]
[Route("api/relatorios")]
public class RelatoriosController : ControllerBase
{
    private readonly LivrosContext _context;

    public RelatoriosController(LivrosContext context) => _context = context;

    /// <summary>
    /// Quantidade de livros lidos num determinado ano, com base na data de
    /// fim de leitura informada nas avaliações.
    /// </summary>
    [HttpGet("livros-lidos/{ano:int}")]
    public async Task<IActionResult> LivrosLidosNoAno(int ano)
    {
        var avaliacoes = await _context.Avaliacoes
            .Include(a => a.Livro)
            .Include(a => a.Usuario)
            .Where(a => a.DataFimLeitura.Year == ano)
            .ToListAsync();

        var porLivro = avaliacoes
            .Where(a => a.Livro is not null)
            .GroupBy(a => a.IdLivro)
            .Select(g => new
            {
                IdLivro = g.Key,
                Titulo = g.First().Livro!.Titulo,
                Autor = g.First().Livro!.Autor,
                QuantidadePaginas = g.First().Livro!.QuantidadePaginas,
                QuantidadeLeituras = g.Count(),
                NotaMedia = Math.Round(g.Average(a => (double)a.Nota), 2)
            })
            .OrderByDescending(x => x.NotaMedia)
            .ToList();

        return Ok(new
        {
            Ano = ano,
            LivrosDistintosLidos = porLivro.Count,
            TotalDeLeituras = avaliacoes.Count,
            PaginasLidas = porLivro.Sum(x => x.QuantidadePaginas),
            LeitoresDistintos = avaliacoes.Select(a => a.IdUsuario).Distinct().Count(),
            Livros = porLivro
        });
    }
}
