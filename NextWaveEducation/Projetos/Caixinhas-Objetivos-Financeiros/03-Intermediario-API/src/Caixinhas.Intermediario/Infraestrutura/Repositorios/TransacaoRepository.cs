using Caixinhas.Intermediario.Dominio.Entidades;
using Caixinhas.Intermediario.Dominio.Interfaces;
using Caixinhas.Intermediario.Infraestrutura.Dados;
using Microsoft.EntityFrameworkCore;

namespace Caixinhas.Intermediario.Infraestrutura.Repositorios;

public class TransacaoRepository : ITransacaoRepository
{
    private readonly CaixinhasDbContext _context;

    public TransacaoRepository(CaixinhasDbContext context) => _context = context;

    public async Task<List<Transacao>> ObterPorObjetivoAsync(int idObjetivo)
        => await _context.Transacoes
            .Where(t => t.ObjetivoFinanceiroId == idObjetivo)
            .AsNoTracking()
            .OrderByDescending(t => t.DataTransacao)
            .ThenByDescending(t => t.Id)
            .ToListAsync();

    public async Task<Transacao?> ObterPorIdAsync(int idObjetivo, int idTransacao)
        => await _context.Transacoes
            .Include(t => t.ObjetivoFinanceiro)
            .FirstOrDefaultAsync(t => t.Id == idTransacao && t.ObjetivoFinanceiroId == idObjetivo);

    /// <summary>
    /// Transações de todas as caixinhas ativas num intervalo de datas.
    /// Base do relatório de evolução.
    /// </summary>
    public async Task<List<Transacao>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim)
        => await _context.Transacoes
            .Include(t => t.ObjetivoFinanceiro)
            .Where(t => t.DataTransacao >= inicio.Date && t.DataTransacao <= fim.Date)
            .AsNoTracking()
            .OrderBy(t => t.DataTransacao)
            .ToListAsync();
}
