namespace Caixinhas.Domain.Repositories;

/// <summary>
/// Unit of Work: agrupa os repositórios numa única transação e centraliza o
/// commit. É também o ponto onde os eventos de domínio acumulados nas
/// entidades são despachados.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IObjetivoRepository Objetivos { get; }
    ITransacaoRepository Transacoes { get; }

    /// <summary>Persiste as alterações e publica os eventos de domínio pendentes.</summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executa a operação dentro de uma transação de banco: ou tudo entra,
    /// ou nada entra. É o que garante que a transação nova e o ValorTotal
    /// atualizado da caixinha nunca fiquem divergentes.
    /// </summary>
    Task<T> ExecutarEmTransacaoAsync<T>(
        Func<CancellationToken, Task<T>> operacao,
        CancellationToken cancellationToken = default);

    Task IniciarTransacaoAsync(CancellationToken cancellationToken = default);
    Task ConfirmarTransacaoAsync(CancellationToken cancellationToken = default);
    Task DesfazerTransacaoAsync(CancellationToken cancellationToken = default);
}
