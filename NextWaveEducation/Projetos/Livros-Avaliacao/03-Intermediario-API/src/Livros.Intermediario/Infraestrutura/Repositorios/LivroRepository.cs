using Livros.Intermediario.Dominio.Entidades;
using Livros.Intermediario.Dominio.Interfaces;
using Livros.Intermediario.Infraestrutura.Dados;
using Microsoft.EntityFrameworkCore;

namespace Livros.Intermediario.Infraestrutura.Repositorios;

public class LivroRepository : ILivroRepository
{
    private readonly LivrosDbContext _context;

    public LivroRepository(LivrosDbContext context) => _context = context;

    public async Task<List<Livro>> ObterTodosAsync(string? busca = null)
    {
        var query = _context.Livros
            .Include(l => l.Avaliacoes)
            .AsNoTracking()
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
            .ToListAsync();
    }

    public async Task<Livro?> ObterPorIdAsync(int id)
        => await _context.Livros.FirstOrDefaultAsync(l => l.Id == id);

    public async Task<Livro?> ObterPorIdComAvaliacoesAsync(int id)
        => await _context.Livros
            .Include(l => l.Avaliacoes)
                .ThenInclude(a => a.Usuario)
            .FirstOrDefaultAsync(l => l.Id == id);

    /// <summary>
    /// Verifica duplicidade de ISBN comparando a versão normalizada (sem hífens).
    /// </summary>
    public async Task<bool> ExisteIsbnAsync(string isbnNormalizado, int? ignorarId = null)
    {
        var candidatos = await _context.Livros
            .Where(l => ignorarId == null || l.Id != ignorarId)
            .Select(l => new { l.Id, l.ISBN })
            .AsNoTracking()
            .ToListAsync();

        return candidatos.Any(c => Livro.NormalizarIsbn(c.ISBN) == isbnNormalizado);
    }

    public async Task AdicionarAsync(Livro livro)
    {
        await _context.Livros.AddAsync(livro);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Livro livro)
    {
        _context.Livros.Update(livro);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(Livro livro)
    {
        _context.Livros.Remove(livro);
        await _context.SaveChangesAsync();
    }
}
