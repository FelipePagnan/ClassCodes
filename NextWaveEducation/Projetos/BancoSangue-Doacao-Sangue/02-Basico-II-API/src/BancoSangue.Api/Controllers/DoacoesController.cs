using BancoSangue.Api.Dados;
using BancoSangue.Api.Dtos;
using BancoSangue.Api.Entidades;
using BancoSangue.Api.Excecoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BancoSangue.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoacoesController : ControllerBase
{
    private const int QuantidadeMinimaML = 420;
    private const int QuantidadeMaximaML = 470;
    private const double PesoMinimoKg = 50;

    private readonly BancoSangueContext _context;

    public DoacoesController(BancoSangueContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DoacaoDto>>> Listar()
    {
        var doacoes = await _context.Doacoes.Include(d => d.Doador).ToListAsync();
        return Ok(doacoes.Select(d => new DoacaoDto(d.Id, d.DoadorId, d.Doador?.NomeCompleto, d.DataDoacao, d.QuantidadeML)));
    }

    [HttpPost]
    public async Task<ActionResult<DoacaoDto>> Registrar(CriarDoacaoDto dto)
    {
        var doador = await _context.Doadores.Include(d => d.Doacoes)
            .FirstOrDefaultAsync(d => d.Id == dto.DoadorId);

        if (doador is null)
            throw new RegraNegocioException($"Doador {dto.DoadorId} não encontrado.");

        if (!doador.EhMaiorDeIdade)
            throw new RegraNegocioException($"'{doador.NomeCompleto}' é menor de idade e não pode doar sangue.");

        if (doador.Peso < PesoMinimoKg)
            throw new RegraNegocioException($"'{doador.NomeCompleto}' pesa {doador.Peso}kg. O peso mínimo para doar é {PesoMinimoKg}kg.");

        if (dto.QuantidadeML < QuantidadeMinimaML || dto.QuantidadeML > QuantidadeMaximaML)
            throw new RegraNegocioException($"A quantidade doada deve estar entre {QuantidadeMinimaML}ml e {QuantidadeMaximaML}ml.");

        var data = dto.DataDoacao ?? DateTime.Now;

        if (doador.UltimaDoacao is DateTime ultima)
        {
            var dias = (data.Date - ultima.Date).TotalDays;
            if (dias < doador.IntervaloMinimoDiasEntreDoacoes)
                throw new RegraNegocioException(
                    $"'{doador.NomeCompleto}' deve aguardar mais {doador.IntervaloMinimoDiasEntreDoacoes - dias:0} dia(s) para doar novamente.");
        }

        var doacao = new Doacao { DoadorId = doador.Id, DataDoacao = data, QuantidadeML = dto.QuantidadeML };
        _context.Doacoes.Add(doacao);

        // Atualiza o estoque sempre que uma doação for registrada.
        var estoque = await _context.EstoqueSangue
            .FirstAsync(e => e.TipoSanguineo == doador.TipoSanguineo && e.FatorRh == doador.FatorRh);
        estoque.QuantidadeML += dto.QuantidadeML;

        await _context.SaveChangesAsync();

        var resultado = new DoacaoDto(doacao.Id, doacao.DoadorId, doador.NomeCompleto, doacao.DataDoacao, doacao.QuantidadeML);
        return CreatedAtAction(nameof(Listar), resultado);
    }
}
