using BancoSangue.Domain.Entities;
using BancoSangue.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BancoSangue.Infrastructure.Persistence.Repositories;

public class DoadorRepository : IDoadorRepository
{
    private readonly BancoSangueDbContext _context;

    public DoadorRepository(BancoSangueDbContext context) => _context = context;

    public async Task<List<Doador>> ObterTodosAsync(CancellationToken cancellationToken = default)
        => await _context.Doadores
            .Include(d => d.Endereco)
            .OrderBy(d => d.NomeCompleto)
            .ToListAsync(cancellationToken);

    public async Task<Doador?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.Doadores
            .Include(d => d.Endereco)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<Doador?> ObterPorIdComDoacoesAsync(int id, CancellationToken cancellationToken = default)
        => await _context.Doadores
            .Include(d => d.Endereco)
            .Include(d => d.Doacoes)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Doadores
            .AnyAsync(d => d.EmailValor == email, cancellationToken);

    public async Task AdicionarAsync(Doador doador, CancellationToken cancellationToken = default)
        => await _context.Doadores.AddAsync(doador, cancellationToken);

    public void Atualizar(Doador doador)
        => _context.Doadores.Update(doador);
}
