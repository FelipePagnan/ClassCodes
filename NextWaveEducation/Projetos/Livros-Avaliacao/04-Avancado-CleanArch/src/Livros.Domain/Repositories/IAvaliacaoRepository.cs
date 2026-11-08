using Livros.Domain.Entities;

namespace Livros.Domain.Repositories;

public interface IAvaliacaoRepository
{
    Task<List<Avaliacao>> ObterPorLivroAsync(int idLivro, CancellationToken ct = default);
    Task<List<Avaliacao>> ObterPorUsuarioAsync(int idUsuario, CancellationToken ct = default);

    /// <summary>Avaliações cujo fim de leitura caiu no ano informado.</summary>
    Task<List<Avaliacao>> ObterPorAnoDeLeituraAsync(int ano, CancellationToken ct = default);

    Task AdicionarAsync(Avaliacao avaliacao, CancellationToken ct = default);
}
