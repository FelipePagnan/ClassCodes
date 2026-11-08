using Caixinhas.Api.Dados;
using Caixinhas.Api.Dtos;
using Caixinhas.Api.Entidades;
using Caixinhas.Api.Enums;
using Caixinhas.Api.Excecoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Caixinhas.Api.Controllers;

[ApiController]
[Route("api/objetivos")]
public class ObjetivosController : ControllerBase
{
    private readonly CaixinhasContext _context;

    public ObjetivosController(CaixinhasContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ObjetivoDto>>> Listar(
        [FromQuery] StatusObjetivo? status)
    {
        // O filtro global de soft delete já exclui os objetivos removidos.
        var query = _context.Objetivos.Include(o => o.Transacoes).AsQueryable();

        if (status is not null)
            query = query.Where(o => o.Status == status);

        var objetivos = await query.ToListAsync();
        return Ok(objetivos.Select(ParaDto).OrderByDescending(o => o.PercentualConcluido));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ObjetivoDetalheDto>> ObterPorId(int id)
    {
        var objetivo = await _context.Objetivos
            .Include(o => o.Transacoes)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (objetivo is null)
            throw new NaoEncontradoException($"Objetivo financeiro {id} não encontrado.");

        return Ok(new ObjetivoDetalheDto(
            objetivo.Id, objetivo.Titulo, objetivo.QuantidadeAlvo, objetivo.ValorTotal,
            objetivo.ValorRestante, objetivo.PercentualConcluido, objetivo.Status,
            objetivo.Prazo, objetivo.DiasRestantes, objetivo.QuantidadeIdealAporteMensal,
            objetivo.DataCriacao,
            objetivo.Transacoes
                .OrderByDescending(t => t.DataTransacao)
                .Select(ParaDto)
                .ToList()));
    }

    [HttpPost]
    public async Task<ActionResult<ObjetivoDto>> Cadastrar(CriarObjetivoDto dto)
    {
        ValidarDados(dto.Titulo, dto.QuantidadeAlvo, dto.Prazo, dto.QuantidadeIdealAporteMensal);

        var titulo = dto.Titulo.Trim();

        if (await _context.Objetivos.AnyAsync(o => o.Titulo == titulo))
            throw new RegraNegocioException($"Já existe um objetivo chamado '{titulo}'.");

        var objetivo = new ObjetivoFinanceiro
        {
            Titulo = titulo,
            QuantidadeAlvo = dto.QuantidadeAlvo,
            Prazo = dto.Prazo?.Date,
            QuantidadeIdealAporteMensal = dto.QuantidadeIdealAporteMensal,
            Status = StatusObjetivo.EmProgresso,
            ValorTotal = 0,
            DataCriacao = DateTime.Now
        };

        _context.Objetivos.Add(objetivo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = objetivo.Id }, ParaDto(objetivo));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ObjetivoDto>> Atualizar(int id, AtualizarObjetivoDto dto)
    {
        var objetivo = await _context.Objetivos
            .Include(o => o.Transacoes)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (objetivo is null)
            throw new NaoEncontradoException($"Objetivo financeiro {id} não encontrado.");

        ValidarDados(dto.Titulo, dto.QuantidadeAlvo, dto.Prazo, dto.QuantidadeIdealAporteMensal);

        var titulo = dto.Titulo.Trim();

        if (await _context.Objetivos.AnyAsync(o => o.Titulo == titulo && o.Id != id))
            throw new RegraNegocioException($"Já existe outro objetivo chamado '{titulo}'.");

        // A meta não pode ficar abaixo do que já foi guardado.
        if (dto.QuantidadeAlvo < objetivo.ValorTotal)
            throw new RegraNegocioException(
                $"A nova meta (R$ {dto.QuantidadeAlvo:N2}) é menor que o saldo atual " +
                $"(R$ {objetivo.ValorTotal:N2}).");

        objetivo.Titulo = titulo;
        objetivo.QuantidadeAlvo = dto.QuantidadeAlvo;
        objetivo.Prazo = dto.Prazo?.Date;
        objetivo.QuantidadeIdealAporteMensal = dto.QuantidadeIdealAporteMensal;

        await _context.SaveChangesAsync();

        return Ok(ParaDto(objetivo));
    }

    /// <summary>Remoção lógica: marca EstaDeletado em vez de apagar o registro.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var objetivo = await _context.Objetivos
            .Include(o => o.Transacoes)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (objetivo is null)
            throw new NaoEncontradoException($"Objetivo financeiro {id} não encontrado.");

        if (objetivo.ValorTotal > 0)
            throw new RegraNegocioException(
                $"'{objetivo.Titulo}' ainda tem R$ {objetivo.ValorTotal:N2} guardados. " +
                "Saque o valor antes de remover o objetivo.");

        objetivo.EstaDeletado = true;

        foreach (var transacao in objetivo.Transacoes)
            transacao.EstaDeletado = true;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id:int}/pausar")]
    public async Task<ActionResult<ObjetivoDto>> Pausar(int id)
        => Ok(await AlterarStatusAsync(id, o => o.Pausar()));

    [HttpPatch("{id:int}/retomar")]
    public async Task<ActionResult<ObjetivoDto>> Retomar(int id)
        => Ok(await AlterarStatusAsync(id, o => o.Retomar()));

    [HttpPatch("{id:int}/cancelar")]
    public async Task<ActionResult<ObjetivoDto>> Cancelar(int id)
        => Ok(await AlterarStatusAsync(id, o => o.Cancelar()));

    private async Task<ObjetivoDto> AlterarStatusAsync(int id, Action<ObjetivoFinanceiro> acao)
    {
        var objetivo = await _context.Objetivos
            .Include(o => o.Transacoes)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (objetivo is null)
            throw new NaoEncontradoException($"Objetivo financeiro {id} não encontrado.");

        acao(objetivo);
        await _context.SaveChangesAsync();

        return ParaDto(objetivo);
    }

    private static ObjetivoDto ParaDto(ObjetivoFinanceiro o) => new(
        o.Id, o.Titulo, o.QuantidadeAlvo, o.ValorTotal, o.ValorRestante,
        o.PercentualConcluido, o.Status, o.Prazo, o.DiasRestantes,
        o.QuantidadeIdealAporteMensal, o.Transacoes.Count, o.DataCriacao);

    private static TransacaoDto ParaDto(Transacao t) => new(
        t.Id, t.ObjetivoFinanceiroId, t.Quantidade, t.Tipo, t.DataTransacao, t.DataCriacao);

    private static void ValidarDados(
        string titulo, decimal quantidadeAlvo, DateTime? prazo, decimal? aporteMensal)
    {
        if (string.IsNullOrWhiteSpace(titulo) || titulo.Trim().Length < 3)
            throw new RegraNegocioException("O título é obrigatório e deve ter ao menos 3 caracteres.");

        if (titulo.Trim().Length > 120)
            throw new RegraNegocioException("O título deve ter no máximo 120 caracteres.");

        if (quantidadeAlvo <= 0)
            throw new RegraNegocioException("A quantidade alvo deve ser maior que zero.");

        if (decimal.Round(quantidadeAlvo, 2) != quantidadeAlvo)
            throw new RegraNegocioException("A quantidade alvo deve ter no máximo 2 casas decimais.");

        if (prazo is not null && prazo.Value.Date <= DateTime.Today)
            throw new RegraNegocioException("O prazo, quando informado, deve ser uma data futura.");

        if (aporteMensal is { } aporte)
        {
            if (aporte <= 0)
                throw new RegraNegocioException("O aporte mensal ideal deve ser maior que zero.");

            if (decimal.Round(aporte, 2) != aporte)
                throw new RegraNegocioException(
                    "O aporte mensal ideal deve ter no máximo 2 casas decimais.");
        }
    }
}
