using CRMDev.Application.Interfaces;
using CRMDev.Domain.Entities;
using CRMDev.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRMDev.Infrastructure.Repositories;

public class ContatoRepository : IContatoRepository
{
    private readonly CRMDevDbContext _db;

    public ContatoRepository(CRMDevDbContext db)
    {
        _db = db;
    }

    public async Task<List<Contato>> ListarAsync(string? busca, CancellationToken ct = default)
    {
        var query = _db.Contatos
            .Include(c => c.AreaAtuacao)
            .Include(c => c.ContatoTags).ThenInclude(ct2 => ct2.Tag)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            query = query.Where(c => c.Nome.Contains(termo) || c.Email.Contains(termo));
        }

        return await query.OrderBy(c => c.Nome).ToListAsync(ct);
    }

    public Task<Contato?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        return _db.Contatos
            .Include(c => c.AreaAtuacao)
            .Include(c => c.ContatoTags).ThenInclude(ct2 => ct2.Tag)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task AdicionarAsync(Contato contato, CancellationToken ct = default)
    {
        await _db.Contatos.AddAsync(contato, ct);
    }

    public void Remover(Contato contato)
    {
        _db.Contatos.Remove(contato);
    }

    public Task<bool> ExisteEmailAsync(string email, Guid? ignorarId = null, CancellationToken ct = default)
    {
        return _db.Contatos.AnyAsync(c => c.Email == email && (ignorarId == null || c.Id != ignorarId), ct);
    }

    public async Task<Tag?> ObterOuCriarTagAsync(string nome, CancellationToken ct = default)
    {
        var nomeNormalizado = nome.Trim();
        var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Nome.ToLower() == nomeNormalizado.ToLower(), ct);
        if (tag is not null) return tag;

        tag = new Tag { Nome = nomeNormalizado };
        await _db.Tags.AddAsync(tag, ct);
        return tag;
    }

    public async Task SalvarAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
