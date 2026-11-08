using BancoSangue.Domain.Entities;
using BancoSangue.Domain.Enums;
using BancoSangue.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BancoSangue.Infrastructure.Persistence.Repositories;

public class EstoqueSangueRepository : IEstoqueSangueRepository
{
    private readonly BancoSangueDbContext _context;

    public EstoqueSangueRepository(BancoSangueDbContext context) => _context = context;

    public async Task<List<EstoqueSangue>> ObterTodosAsync(CancellationToken cancellationToken = default)
        => await _context.EstoquesSangue
            .OrderBy(e => e.TipoSanguineo)
            .ThenBy(e => e.FatorRh)
            .ToListAsync(cancellationToken);

    public async Task<EstoqueSangue?> ObterPorTipoAsync(
        TipoSanguineo tipo,
        FatorRh fator,
        CancellationToken cancellationToken = default)
        => await _context.EstoquesSangue
            .FirstOrDefaultAsync(e => e.TipoSanguineo == tipo && e.FatorRh == fator, cancellationToken);

    public async Task<List<EstoqueSangue>> ObterAbaixoDoMinimoAsync(CancellationToken cancellationToken = default)
        => await _context.EstoquesSangue
            .Where(e => e.QuantidadeML < EstoqueSangue.QuantidadeMinimaML)
            .OrderBy(e => e.QuantidadeML)
            .ToListAsync(cancellationToken);

    public void Atualizar(EstoqueSangue estoque)
        => _context.EstoquesSangue.Update(estoque);
}
