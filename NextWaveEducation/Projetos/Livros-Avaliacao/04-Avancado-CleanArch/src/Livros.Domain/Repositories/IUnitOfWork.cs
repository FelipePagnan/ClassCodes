namespace Livros.Domain.Repositories;

/// <summary>
/// Unit of Work: agrupa os repositórios numa única transação e centraliza
/// o commit. É também o ponto onde os eventos de domínio acumulados nas
/// entidades são despachados.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    ILivroRepository Livros { get; }
    IUsuarioRepository Usuarios { get; }
    IAvaliacaoRepository Avaliacoes { get; }

    /// <summary>Persiste as alterações e publica os eventos de domínio pendentes.</summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    Task IniciarTransacaoAsync(CancellationToken cancellationToken = default);
    Task ConfirmarTransacaoAsync(CancellationToken cancellationToken = default);
    Task DesfazerTransacaoAsync(CancellationToken cancellationToken = default);
}
