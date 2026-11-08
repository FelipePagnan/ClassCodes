using Caixinhas.Api.Dados;
using Caixinhas.Api.Dtos;
using Caixinhas.Api.Entidades;
using Caixinhas.Api.Excecoes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Caixinhas.Api.Controllers;

/// <summary>Cadastro, listagem, detalhes e remoção de transações.</summary>
[ApiController]
[Route("api/objetivos/{idObjetivo:int}/transacoes")]
public class TransacoesController : ControllerBase
{
    private readonly CaixinhasContext _context;

    public TransacoesController(CaixinhasContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TransacaoDto>>> Listar(int idObjetivo)
    {
        if (!await _context.Objetivos.AnyAsync(o => o.Id == idObjetivo))
            throw new NaoEncontradoException($"Objetivo financeiro {idObjetivo} não encontrado.");

        var transacoes = await _context.Transacoes
            .Where(t => t.ObjetivoFinanceiroId == idObjetivo)
            .OrderByDescending(t => t.DataTransacao)
            .ThenByDescending(t => t.Id)
            .ToListAsync();

        return Ok(transacoes.Select(ParaDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TransacaoDto>> ObterPorId(int idObjetivo, int id)
    {
        var transacao = await _context.Transacoes
            .FirstOrDefaultAsync(t => t.Id == id && t.ObjetivoFinanceiroId == idObjetivo);

        if (transacao is null)
            throw new NaoEncontradoException(
                $"Transação {id} não encontrada no objetivo {idObjetivo}.");

        return Ok(ParaDto(transacao));
    }

    /// <summary>
    /// Cadastra a transação. O ValorTotal do objetivo é atualizado na mesma
    /// operação, dentro de uma transação de banco.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ResultadoTransacaoDto>> Cadastrar(
        int idObjetivo, CriarTransacaoDto dto)
    {
        var objetivo = await _context.Objetivos
            .Include(o => o.Transacoes)
            .FirstOrDefaultAsync(o => o.Id == idObjetivo);

        if (objetivo is null)
            throw new NaoEncontradoException($"Objetivo financeiro {idObjetivo} não encontrado.");

        // A entidade valida a quantidade, a data e o saldo, e atualiza o ValorTotal.
        var transacao = objetivo.RegistrarTransacao(
            dto.Quantidade, dto.Tipo, dto.DataTransacao ?? DateTime.Today);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(ObterPorId),
            new { idObjetivo, id = transacao.Id },
            new ResultadoTransacaoDto(
                ParaDto(transacao),
                objetivo.Titulo,
                objetivo.ValorTotal,
                objetivo.PercentualConcluido,
                objetivo.Status,
                objetivo.MetaAlcancada));
    }

    /// <summary>Remoção lógica da transação, ajustando o ValorTotal do objetivo.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int idObjetivo, int id)
    {
        var objetivo = await _context.Objetivos
            .Include(o => o.Transacoes)
            .FirstOrDefaultAsync(o => o.Id == idObjetivo);

        if (objetivo is null)
            throw new NaoEncontradoException($"Objetivo financeiro {idObjetivo} não encontrado.");

        var transacao = objetivo.Transacoes.FirstOrDefault(t => t.Id == id)
            ?? throw new NaoEncontradoException(
                $"Transação {id} não encontrada no objetivo {idObjetivo}.");

        objetivo.RemoverTransacao(transacao);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static TransacaoDto ParaDto(Transacao t) => new(
        t.Id, t.ObjetivoFinanceiroId, t.Quantidade, t.Tipo, t.DataTransacao, t.DataCriacao);
}
