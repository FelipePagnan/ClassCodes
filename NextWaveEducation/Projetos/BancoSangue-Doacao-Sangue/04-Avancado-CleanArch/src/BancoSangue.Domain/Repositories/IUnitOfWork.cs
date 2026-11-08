namespace BancoSangue.Domain.Repositories;

/// <summary>
/// Unit of Work: agrupa os repositórios em uma única transação e
/// centraliza o commit. É também o ponto onde os eventos de domínio
/// acumulados nas entidades são despachados.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IDoadorRepository Doadores { get; }
    IDoacaoRepository Doacoes { get; }
    IEstoqueSangueRepository EstoquesSangue { get; }

    /// <summary>Persiste as alterações e publica os eventos de domínio pendentes.</summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    Task IniciarTransacaoAsync(CancellationToken cancellationToken = default);
    Task ConfirmarTransacaoAsync(CancellationToken cancellationToken = default);
    Task DesfazerTransacaoAsync(CancellationToken cancellationToken = default);
}
