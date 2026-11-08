using Livros.Domain.Entities;
using Livros.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Livros.Infrastructure.Persistence.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly LivrosDbContext _context;

    public UsuarioRepository(LivrosDbContext context) => _context = context;

    public async Task<List<Usuario>> ObterTodosAsync(CancellationToken ct = default)
        => await _context.Usuarios
            .Include(u => u.Avaliacoes)
            .OrderBy(u => u.Nome)
            .ToListAsync(ct);

    public async Task<Usuario?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<Usuario?> ObterPorIdComAvaliacoesAsync(int id, CancellationToken ct = default)
        => await _context.Usuarios
            .Include(u => u.Avaliacoes)
                .ThenInclude(a => a.Livro)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<bool> ExisteEmailAsync(
        string email, int? ignorarId = null, CancellationToken ct = default)
    {
        var normalizado = email.Trim().ToLowerInvariant();

        return await _context.Usuarios
            .AnyAsync(u => u.EmailValor == normalizado
                        && (ignorarId == null || u.Id != ignorarId), ct);
    }

    public async Task AdicionarAsync(Usuario usuario, CancellationToken ct = default)
        => await _context.Usuarios.AddAsync(usuario, ct);

    public void Atualizar(Usuario usuario) => _context.Usuarios.Update(usuario);

    public void Remover(Usuario usuario) => _context.Usuarios.Remove(usuario);
}
