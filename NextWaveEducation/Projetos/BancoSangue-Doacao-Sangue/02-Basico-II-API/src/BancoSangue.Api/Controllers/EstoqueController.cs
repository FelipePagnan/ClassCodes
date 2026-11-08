using BancoSangue.Api.Dados;
using BancoSangue.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BancoSangue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstoqueController : ControllerBase
{
    private readonly BancoSangueContext _context;

    public EstoqueController(BancoSangueContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EstoqueDto>>> Listar()
    {
        var estoque = await _context.EstoqueSangue.ToListAsync();
        return Ok(estoque.Select(e => new EstoqueDto(e.TipoSanguineo, e.FatorRh, e.QuantidadeML, e.EstoqueBaixo)));
    }

    [HttpGet("baixo")]
    public async Task<ActionResult<IEnumerable<EstoqueDto>>> ListarEstoqueBaixo()
    {
        var estoque = await _context.EstoqueSangue.ToListAsync();
        return Ok(estoque.Where(e => e.EstoqueBaixo).Select(e => new EstoqueDto(e.TipoSanguineo, e.FatorRh, e.QuantidadeML, e.EstoqueBaixo)));
    }
}
