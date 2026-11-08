using Livros.Intermediario.Dominio.Entidades;
using Livros.Intermediario.Dominio.Interfaces;
using Livros.Intermediario.Infraestrutura.Dados;
using Microsoft.EntityFrameworkCore;

namespace Livros.Intermediario.Infraestrutura.Repositorios;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly LivrosDbContext _context;

    public UsuarioRepository(LivrosDbContext context) => _context = context;

    public async Task<List<Usuario>> ObterTodosAsync()
        => await _context.Usuarios
            .Include(u => u.Avaliacoes)
            .AsNoTracking()
            .OrderBy(u => u.Nome)
            .ToListAsync();

    public async Task<Usuario?> ObterPorIdAsync(int id)
        => await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<Usuario?> ObterPorIdComAvaliacoesAsync(int id)
        => await _context.Usuarios
            .Include(u => u.Avaliacoes)
                .ThenInclude(a => a.Livro)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<bool> ExisteEmailAsync(string email, int? ignorarId = null)
    {
        var normalizado = email.Trim().ToLowerInvariant();
        return await _context.Usuarios
            .AnyAsync(u => u.Email == normalizado && (ignorarId == null || u.Id != ignorarId));
    }

    public async Task AdicionarAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(Usuario usuario)
    {
        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();
    }
}
