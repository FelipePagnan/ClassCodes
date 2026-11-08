using CRMDev.Application.Interfaces;
using CRMDev.Domain.Entities;
using CRMDev.Domain.Enums;
using CRMDev.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMDev.Infrastructure.Repositories;

public class OportunidadeRepository : IOportunidadeRepository
{
    private readonly CRMDevDbContext _db;

    public OportunidadeRepository(CRMDevDbContext db)
    {
        _db = db;
    }

    public async Task<List<Oportunidade>> ListarAsync(EtapaOportunidade? etapa, StatusOportunidade? status, Guid? contatoId, CancellationToken ct = default)
    {
        var query = _db.Oportunidades
            .Include(o => o.Contato)
            .AsNoTracking()
            .AsQueryable();

        if (etapa.HasValue) query = query.Where(o => o.Etapa == etapa.Value);
        if (status.HasValue) query = query.Where(o => o.Status == status.Value);
        if (contatoId.HasValue) query = query.Where(o => o.ContatoId == contatoId.Value);

        return await query.OrderByDescending(o => o.CriadoEm).ToListAsync(ct);
    }

    public Task<Oportunidade?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        return _db.Oportunidades
            .Include(o => o.Contato)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task AdicionarAsync(Oportunidade oportunidade, CancellationToken ct = default)
    {
        await _db.Oportunidades.AddAsync(oportunidade, ct);
    }

    public void Remover(Oportunidade oportunidade)
    {
        _db.Oportunidades.Remove(oportunidade);
    }

    public async Task SalvarAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
