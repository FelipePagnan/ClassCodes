using Livros.Intermediario.Dominio.Entidades;
using Livros.Intermediario.Dominio.Interfaces;
using Livros.Intermediario.Infraestrutura.Dados;
using Microsoft.EntityFrameworkCore;

namespace Livros.Intermediario.Infraestrutura.Repositorios;

public class AvaliacaoRepository : IAvaliacaoRepository
{
    private readonly LivrosDbContext _context;

    public AvaliacaoRepository(LivrosDbContext context) => _context = context;

    public async Task<List<Avaliacao>> ObterPorLivroAsync(int idLivro)
        => await _context.Avaliacoes
            .Include(a => a.Usuario)
            .Where(a => a.IdLivro == idLivro)
            .AsNoTracking()
            .OrderByDescending(a => a.DataCriacao)
            .ToListAsync();

    public async Task<List<Avaliacao>> ObterPorUsuarioAsync(int idUsuario)
        => await _context.Avaliacoes
            .Include(a => a.Livro)
            .Where(a => a.IdUsuario == idUsuario)
            .AsNoTracking()
            .OrderByDescending(a => a.DataFimLeitura)
            .ToListAsync();

    /// <summary>
    /// Avaliações cujo fim de leitura caiu no ano informado.
    /// Base do relatório de livros lidos no ano.
    /// </summary>
    public async Task<List<Avaliacao>> ObterPorAnoDeLeituraAsync(int ano)
        => await _context.Avaliacoes
            .Include(a => a.Livro)
            .Include(a => a.Usuario)
            .Where(a => a.DataFimLeitura.Year == ano)
            .AsNoTracking()
            .OrderByDescending(a => a.DataFimLeitura)
            .ToListAsync();

    public async Task AdicionarAsync(Avaliacao avaliacao)
    {
        await _context.Avaliacoes.AddAsync(avaliacao);
        await _context.SaveChangesAsync();
    }
}
