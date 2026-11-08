using Livros.Intermediario.Dominio.Entidades;

namespace Livros.Intermediario.Dominio.Interfaces;

public interface IUsuarioRepository
{
    Task<List<Usuario>> ObterTodosAsync();
    Task<Usuario?> ObterPorIdAsync(int id);
    Task<Usuario?> ObterPorIdComAvaliacoesAsync(int id);
    Task<bool> ExisteEmailAsync(string email, int? ignorarId = null);
    Task AdicionarAsync(Usuario usuario);
    Task AtualizarAsync(Usuario usuario);
    Task RemoverAsync(Usuario usuario);
}
