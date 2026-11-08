using BancoSangue.Intermediario.Dominio.Entidades;
using BancoSangue.Intermediario.Dominio.Enums;
using BancoSangue.Intermediario.Dominio.Interfaces;
using BancoSangue.Intermediario.Infraestrutura.Dados;
using Microsoft.EntityFrameworkCore;

namespace BancoSangue.Intermediario.Infraestrutura.Repositorios;

public class EstoqueSangueRepository : IEstoqueSangueRepository
{
    private readonly BancoSangueDbContext _context;

    public EstoqueSangueRepository(BancoSangueDbContext context) => _context = context;

    public async Task<List<EstoqueSangue>> ObterTodosAsync()
        => await _context.EstoquesSangue.AsNoTracking().ToListAsync();

    public async Task<EstoqueSangue?> ObterPorTipoAsync(TipoSanguineo tipo, FatorRh fator)
        => await _context.EstoquesSangue
            .FirstOrDefaultAsync(e => e.TipoSanguineo == tipo && e.FatorRh == fator);

    public async Task<List<EstoqueSangue>> ObterComEstoqueBaixoAsync()
    {
        // EstoqueBaixo é propriedade calculada (não mapeada), então a comparação é feita pela coluna.
        return await _context.EstoquesSangue
            .Where(e => e.QuantidadeML < EstoqueSangue.QuantidadeMinimaML)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AtualizarAsync(EstoqueSangue estoque)
    {
        _context.EstoquesSangue.Update(estoque);
        await _context.SaveChangesAsync();
    }
}
