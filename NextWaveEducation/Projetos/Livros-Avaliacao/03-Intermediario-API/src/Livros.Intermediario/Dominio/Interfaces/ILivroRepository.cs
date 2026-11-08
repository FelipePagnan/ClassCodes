using Livros.Intermediario.Dominio.Entidades;

namespace Livros.Intermediario.Dominio.Interfaces;

public interface ILivroRepository
{
    Task<List<Livro>> ObterTodosAsync(string? busca = null);
    Task<Livro?> ObterPorIdAsync(int id);
    Task<Livro?> ObterPorIdComAvaliacoesAsync(int id);
    Task<bool> ExisteIsbnAsync(string isbnNormalizado, int? ignorarId = null);
    Task AdicionarAsync(Livro livro);
    Task AtualizarAsync(Livro livro);
    Task RemoverAsync(Livro livro);
}
