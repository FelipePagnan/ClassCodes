using Caixinhas.Intermediario.Dominio.Entidades;
using Caixinhas.Intermediario.Dominio.Enums;
using Caixinhas.Intermediario.Dominio.Interfaces;
using Caixinhas.Intermediario.Infraestrutura.Dados;
using Microsoft.EntityFrameworkCore;

namespace Caixinhas.Intermediario.Infraestrutura.Repositorios;

public class ObjetivoRepository : IObjetivoRepository
{
    private readonly CaixinhasDbContext _context;
    private readonly ILogger<ObjetivoRepository> _logger;

    public ObjetivoRepository(CaixinhasDbContext context, ILogger<ObjetivoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ObjetivoFinanceiro>> ObterTodosAsync(StatusObjetivo? status = null)
    {
        // O filtro global de soft delete já exclui os objetivos removidos,
        // e o Include traz apenas as transações ativas.
        var query = _context.Objetivos
            .Include(o => o.Transacoes)
            .AsQueryable();

        if (status is not null)
            query = query.Where(o => o.Status == status);

        return await query
            .OrderByDescending(o => o.ValorTotal)
            .ThenBy(o => o.Titulo)
            .ToListAsync();
    }

    public async Task<ObjetivoFinanceiro?> ObterPorIdAsync(int id)
        => await _context.Objetivos.FirstOrDefaultAsync(o => o.Id == id);

    public async Task<ObjetivoFinanceiro?> ObterPorIdComTransacoesAsync(int id)
        => await _context.Objetivos
            .Include(o => o.Transacoes)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<bool> ExisteTituloAsync(string titulo, int? ignorarId = null)
    {
        var normalizado = titulo.Trim();

        return await _context.Objetivos
            .AnyAsync(o => o.Titulo == normalizado && (ignorarId == null || o.Id != ignorarId));
    }

    public async Task AdicionarAsync(ObjetivoFinanceiro objetivo)
    {
        await _context.Objetivos.AddAsync(objetivo);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Salva dentro de uma transação de banco. A transação nova e o ValorTotal
    /// atualizado do objetivo são gravados juntos: ou os dois entram, ou
    /// nenhum entra. Sem isso, uma falha no meio deixaria o saldo divergente
    /// do histórico de transações.
    /// </summary>
    public async Task SalvarComTransacaoAsync(ObjetivoFinanceiro objetivo)
    {
        await using var transacaoDb = await _context.Database.BeginTransactionAsync();

        try
        {
            await _context.SaveChangesAsync();
            await transacaoDb.CommitAsync();
        }
        catch (Exception ex)
        {
            await transacaoDb.RollbackAsync();

            _logger.LogError(ex,
                "Falha ao salvar o objetivo {ObjetivoId}. Alterações desfeitas.", objetivo.Id);

            throw;
        }
    }

    public async Task SalvarAsync() => await _context.SaveChangesAsync();
}
