using BancoSangue.Domain.Entities;
using BancoSangue.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BancoSangue.Infrastructure.Persistence.Repositories;

public class DoacaoRepository : IDoacaoRepository
{
    private readonly BancoSangueDbContext _context;

    public DoacaoRepository(BancoSangueDbContext context) => _context = context;

    public async Task<List<Doacao>> ObterTodasAsync(CancellationToken cancellationToken = default)
        => await _context.Doacoes
            .Include(d => d.Doador)
                .ThenInclude(d => d.Endereco)
            .OrderByDescending(d => d.DataDoacao)
            .ToListAsync(cancellationToken);

    public async Task<List<Doacao>> ObterPorDoadorAsync(int doadorId, CancellationToken cancellationToken = default)
        => await _context.Doacoes
            .Where(d => d.DoadorId == doadorId)
            .OrderByDescending(d => d.DataDoacao)
            .ToListAsync(cancellationToken);

    /// <summary>Doações a partir de uma data. Usado no relatório dos últimos 30 dias.</summary>
    public async Task<List<Doacao>> ObterApartirDeAsync(
        DateTime dataInicial,
        CancellationToken cancellationToken = default)
        => await _context.Doacoes
            .Include(d => d.Doador)
                .ThenInclude(d => d.Endereco)
            .Where(d => d.DataDoacao >= dataInicial)
            .OrderByDescending(d => d.DataDoacao)
            .ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Doacao doacao, CancellationToken cancellationToken = default)
        => await _context.Doacoes.AddAsync(doacao, cancellationToken);
}
