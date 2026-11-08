using BancoSangue.Intermediario.Dominio.Entidades;
using BancoSangue.Intermediario.Dominio.Interfaces;
using BancoSangue.Intermediario.Infraestrutura.Dados;
using Microsoft.EntityFrameworkCore;

namespace BancoSangue.Intermediario.Infraestrutura.Repositorios;

public class DoadorRepository : IDoadorRepository
{
    private readonly BancoSangueDbContext _context;

    public DoadorRepository(BancoSangueDbContext context) => _context = context;

    public async Task<List<Doador>> ObterTodosAsync()
        => await _context.Doadores
            .Include(d => d.Endereco)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Doador?> ObterPorIdAsync(int id)
        => await _context.Doadores
            .Include(d => d.Endereco)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Doador?> ObterPorIdComDoacoesAsync(int id)
        => await _context.Doadores
            .Include(d => d.Endereco)
            .Include(d => d.Doacoes)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Doador?> ObterPorEmailAsync(string email)
        => await _context.Doadores.FirstOrDefaultAsync(d => d.Email == email);

    public async Task<bool> ExisteEmailAsync(string email)
        => await _context.Doadores.AnyAsync(d => d.Email == email);

    public async Task AdicionarAsync(Doador doador)
    {
        await _context.Doadores.AddAsync(doador);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Doador doador)
    {
        _context.Doadores.Update(doador);
        await _context.SaveChangesAsync();
    }
}
