using BancoSangue.Intermediario.Dominio.Entidades;

namespace BancoSangue.Intermediario.Dominio.Interfaces;

public interface IDoadorRepository
{
    Task<List<Doador>> ObterTodosAsync();
    Task<Doador?> ObterPorIdAsync(int id);
    Task<Doador?> ObterPorIdComDoacoesAsync(int id);
    Task<Doador?> ObterPorEmailAsync(string email);
    Task<bool> ExisteEmailAsync(string email);
    Task AdicionarAsync(Doador doador);
    Task AtualizarAsync(Doador doador);
}
