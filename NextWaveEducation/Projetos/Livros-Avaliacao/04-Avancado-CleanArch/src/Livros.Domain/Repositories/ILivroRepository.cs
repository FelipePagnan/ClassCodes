using Livros.Domain.Entities;

namespace Livros.Domain.Repositories;

public interface ILivroRepository
{
    Task<List<Livro>> ObterTodosAsync(string? busca = null, CancellationToken ct = default);
    Task<Livro?> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<Livro?> ObterPorIdComAvaliacoesAsync(int id, CancellationToken ct = default);
    Task<bool> ExisteIsbnAsync(string isbnNormalizado, int? ignorarId = null, CancellationToken ct = default);
    Task AdicionarAsync(Livro livro, CancellationToken ct = default);
    void Atualizar(Livro livro);
    void Remover(Livro livro);
}
