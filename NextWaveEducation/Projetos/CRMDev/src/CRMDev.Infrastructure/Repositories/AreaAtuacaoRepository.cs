using CRMDev.Domain.Entities;
using CRMDev.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMDev.Infrastructure.Repositories;

public interface IAreaAtuacaoRepository
{
    Task<List<AreaAtuacao>> ListarAsync(CancellationToken ct = default);
    Task<AreaAtuacao> CriarAsync(string nome, CancellationToken ct = default);
}

public class AreaAtuacaoRepository : IAreaAtuacaoRepository
{
    private readonly CRMDevDbContext _db;

    public AreaAtuacaoRepository(CRMDevDbContext db)
    {
        _db = db;
    }

    public Task<List<AreaAtuacao>> ListarAsync(CancellationToken ct = default)
    {
        return _db.AreasAtuacao.AsNoTracking().OrderBy(a => a.Nome).ToListAsync(ct);
    }

    public async Task<AreaAtuacao> CriarAsync(string nome, CancellationToken ct = default)
    {
        var existente = await _db.AreasAtuacao.FirstOrDefaultAsync(a => a.Nome.ToLower() == nome.ToLower(), ct);
        if (existente is not null) return existente;

        var area = new AreaAtuacao { Nome = nome.Trim() };
        await _db.AreasAtuacao.AddAsync(area, ct);
        await _db.SaveChangesAsync(ct);
        return area;
    }
}
