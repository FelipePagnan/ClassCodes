using Livros.Domain.Entities;
using Livros.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Livros.Infrastructure.Persistence.Repositories;

public class AvaliacaoRepository : IAvaliacaoRepository
{
    private readonly LivrosDbContext _context;

    public AvaliacaoRepository(LivrosDbContext context) => _context = context;

    public async Task<List<Avaliacao>> ObterPorLivroAsync(int idLivro, CancellationToken ct = default)
        => await _context.Avaliacoes
            .Include(a => a.Usuario)
            .Where(a => a.IdLivro == idLivro)
            .OrderByDescending(a => a.DataCriacao)
            .ToListAsync(ct);

    public async Task<List<Avaliacao>> ObterPorUsuarioAsync(int idUsuario, CancellationToken ct = default)
        => await _context.Avaliacoes
            .Include(a => a.Livro)
            .Where(a => a.IdUsuario == idUsuario)
            .OrderByDescending(a => a.DataFimLeitura)
            .ToListAsync(ct);

    /// <summary>
    /// Avaliações cujo fim de leitura caiu no ano informado.
    /// Base do relatório de livros lidos no ano.
    /// </summary>
    public async Task<List<Avaliacao>> ObterPorAnoDeLeituraAsync(int ano, CancellationToken ct = default)
        => await _context.Avaliacoes
            .Include(a => a.Livro)
            .Include(a => a.Usuario)
            .Where(a => a.DataFimLeitura.Year == ano)
            .OrderByDescending(a => a.DataFimLeitura)
            .ToListAsync(ct);

    public async Task AdicionarAsync(Avaliacao avaliacao, CancellationToken ct = default)
        => await _context.Avaliacoes.AddAsync(avaliacao, ct);
}
