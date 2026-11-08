using Caixinhas.Domain.Entities;
using Caixinhas.Domain.Enums;
using Caixinhas.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Caixinhas.Infrastructure.Persistence.Repositories;

public class ObjetivoRepository : IObjetivoRepository
{
    private readonly CaixinhasDbContext _context;

    public ObjetivoRepository(CaixinhasDbContext context) => _context = context;

    public async Task<List<ObjetivoFinanceiro>> ObterTodosAsync(
        StatusObjetivo? status = null, CancellationToken ct = default)
    {
        // O filtro global de soft delete já exclui os removidos, e o Include
        // traz apenas as transações ativas.
        var query = _context.Objetivos
            .Include(o => o.Transacoes)
            .AsQueryable();

        if (status is not null)
            query = query.Where(o => o.Status == status);

        return await query
            .OrderByDescending(o => o.ValorTotal)
            .ThenBy(o => o.Titulo)
            .ToListAsync(ct);
    }

    public async Task<ObjetivoFinanceiro?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => await _context.Objetivos.FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<ObjetivoFinanceiro?> ObterPorIdComTransacoesAsync(
        int id, CancellationToken ct = default)
        => await _context.Objetivos
            .Include(o => o.Transacoes)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<bool> ExisteTituloAsync(
        string titulo, int? ignorarId = null, CancellationToken ct = default)
    {
        var normalizado = titulo.Trim();

        return await _context.Objetivos
            .AnyAsync(o => o.Titulo == normalizado && (ignorarId == null || o.Id != ignorarId), ct);
    }

    public async Task AdicionarAsync(ObjetivoFinanceiro objetivo, CancellationToken ct = default)
        => await _context.Objetivos.AddAsync(objetivo, ct);

    public void Atualizar(ObjetivoFinanceiro objetivo) => _context.Objetivos.Update(objetivo);
}
