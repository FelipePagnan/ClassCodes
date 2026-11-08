using BancoSangue.Api.Dados;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BancoSangue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RelatoriosController : ControllerBase
{
    private readonly BancoSangueContext _context;

    public RelatoriosController(BancoSangueContext context) => _context = context;

    [HttpGet("estoque-por-tipo")]
    public async Task<IActionResult> EstoquePorTipo()
    {
        var estoque = await _context.EstoqueSangue.ToListAsync();
        var relatorio = estoque
            .GroupBy(e => $"{e.TipoSanguineo}{(e.FatorRh == Enums.FatorRh.Positivo ? "+" : "-")}")
            .Select(g => new { TipoSanguineo = g.Key, QuantidadeTotalML = g.Sum(e => e.QuantidadeML) });

        return Ok(new
        {
            TotalGeralML = estoque.Sum(e => e.QuantidadeML),
            PorTipo = relatorio
        });
    }

    [HttpGet("doacoes-ultimos-30-dias")]
    public async Task<IActionResult> DoacoesUltimos30Dias()
    {
        var limite = DateTime.Now.AddDays(-30);
        var doacoes = await _context.Doacoes
            .Include(d => d.Doador)
            .Where(d => d.DataDoacao >= limite)
            .OrderByDescending(d => d.DataDoacao)
            .Select(d => new
            {
                d.Id,
                d.DataDoacao,
                d.QuantidadeML,
                Doador = d.Doador == null ? null : new
                {
                    d.Doador.Id,
                    d.Doador.NomeCompleto,
                    d.Doador.Email,
                    TipoSanguineo = $"{d.Doador.TipoSanguineo}{(d.Doador.FatorRh == Enums.FatorRh.Positivo ? "+" : "-")}"
                }
            })
            .ToListAsync();

        return Ok(doacoes);
    }
}
