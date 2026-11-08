using BancoSangue.Intermediario.Dominio.Entidades;
using BancoSangue.Intermediario.Dominio.Interfaces;
using BancoSangue.Intermediario.Infraestrutura.Dados;
using Microsoft.EntityFrameworkCore;

namespace BancoSangue.Intermediario.Infraestrutura.Repositorios;

public class DoacaoRepository : IDoacaoRepository
{
    private readonly BancoSangueDbContext _context;

    public DoacaoRepository(BancoSangueDbContext context) => _context = context;

    public async Task<List<Doacao>> ObterTodasAsync()
        => await _context.Doacoes
            .Include(d => d.Doador)
            .AsNoTracking()
            .OrderByDescending(d => d.DataDoacao)
            .ToListAsync();

    public async Task<List<Doacao>> ObterPorDoadorAsync(int doadorId)
        => await _context.Doacoes
            .Where(d => d.DoadorId == doadorId)
            .AsNoTracking()
            .OrderByDescending(d => d.DataDoacao)
            .ToListAsync();

    public async Task<List<Doacao>> ObterUltimos30DiasAsync()
    {
        var limite = DateTime.Now.AddDays(-30);
        return await _context.Doacoes
            .Include(d => d.Doador)
                .ThenInclude(d => d!.Endereco)
            .Where(d => d.DataDoacao >= limite)
            .AsNoTracking()
            .OrderByDescending(d => d.DataDoacao)
            .ToListAsync();
    }

    public async Task AdicionarAsync(Doacao doacao)
    {
        await _context.Doacoes.AddAsync(doacao);
        await _context.SaveChangesAsync();
    }
}
