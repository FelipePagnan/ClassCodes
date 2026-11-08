using BancoSangue.Domain.Entities;

namespace BancoSangue.Domain.Repositories;

public interface IDoadorRepository
{
    Task<List<Doador>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task<Doador?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Doador?> ObterPorIdComDoacoesAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Doador doador, CancellationToken cancellationToken = default);
    void Atualizar(Doador doador);
}
