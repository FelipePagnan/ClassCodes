using Livros.Domain.Entities;

namespace Livros.Domain.Repositories;

public interface IUsuarioRepository
{
    Task<List<Usuario>> ObterTodosAsync(CancellationToken ct = default);
    Task<Usuario?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<Usuario?> ObterPorIdComAvaliacoesAsync(int id, CancellationToken ct = default);
    Task<bool> ExisteEmailAsync(string email, int? ignorarId = null, CancellationToken ct = default);
    Task AdicionarAsync(Usuario usuario, CancellationToken ct = default);
    void Atualizar(Usuario usuario);
    void Remover(Usuario usuario);
}
