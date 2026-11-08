using Caixinhas.Domain.Entities;
using Caixinhas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Caixinhas.Infrastructure.Persistence.Repositories;

public class TransacaoRepository : ITransacaoRepository
{
    private readonly CaixinhasDbContext _context;

    public TransacaoRepository(CaixinhasDbContext context) => _context = context;

    public async Task<List<Transacao>> ObterPorObjetivoAsync(
        int idObjetivo, CancellationToken ct = default)
        => await _context.Transacoes
            .Where(t => t.ObjetivoFinanceiroId == idObjetivo)
            .OrderByDescending(t => t.DataTransacao)
            .ThenByDescending(t => t.Id)
            .ToListAsync(ct);

    public async Task<Transacao?> ObterPorIdAsync(
        int idObjetivo, int idTransacao, CancellationToken ct = default)
        => await _context.Transacoes
            .FirstOrDefaultAsync(
                t => t.Id == idTransacao && t.ObjetivoFinanceiroId == idObjetivo, ct);

    /// <summary>
    /// Transações de todas as caixinhas ativas num intervalo de datas.
    /// Base do relatório de evolução.
    /// </summary>
    public async Task<List<Transacao>> ObterPorPeriodoAsync(
        DateTime inicio, DateTime fim, CancellationToken ct = default)
        => await _context.Transacoes
            .Include(t => t.Objetivo)
            .Where(t => t.DataTransacao >= inicio.Date && t.DataTransacao <= fim.Date)
            .OrderBy(t => t.DataTransacao)
            .ToListAsync(ct);
}
