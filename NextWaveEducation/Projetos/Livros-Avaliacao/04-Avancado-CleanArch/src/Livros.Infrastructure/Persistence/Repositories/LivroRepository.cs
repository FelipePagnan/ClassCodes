using Livros.Domain.Entities;
using Livros.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Livros.Infrastructure.Persistence.Repositories;

public class LivroRepository : ILivroRepository
{
    private readonly LivrosDbContext _context;

    public LivroRepository(LivrosDbContext context) => _context = context;

    public async Task<List<Livro>> ObterTodosAsync(string? busca = null, CancellationToken ct = default)
    {
        var query = _context.Livros
            .Include(l => l.Avaliacoes)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            query = query.Where(l =>
                l.Titulo.Contains(termo) ||
                l.Autor.Contains(termo) ||
                l.Editora.Contains(termo));
        }

        return await query
            .OrderByDescending(l => l.NotaMedia)
            .ThenBy(l => l.Titulo)
            .ToListAsync(ct);
    }

    public async Task<Livro?> ObterPorIdAsync(int id, CancellationToken ct = default)
        => await _context.Livros.FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<Livro?> ObterPorIdComAvaliacoesAsync(int id, CancellationToken ct = default)
        => await _context.Livros
            .Include(l => l.Avaliacoes)
                .ThenInclude(a => a.Usuario)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

    /// <summary>
    /// Verifica duplicidade comparando o ISBN normalizado, que é persistido
    /// em coluna própria com índice único.
    /// </summary>
    public async Task<bool> ExisteIsbnAsync(
        string isbnNormalizado, int? ignorarId = null, CancellationToken ct = default)
        => await _context.Livros
            .AnyAsync(l => l.IsbnNormalizado == isbnNormalizado
                        && (ignorarId == null || l.Id != ignorarId), ct);

    public async Task AdicionarAsync(Livro livro, CancellationToken ct = default)
        => await _context.Livros.AddAsync(livro, ct);

    public void Atualizar(Livro livro) => _context.Livros.Update(livro);

    public void Remover(Livro livro) => _context.Livros.Remove(livro);
}
