using Caixinhas.Application.Common.Interfaces;
using Caixinhas.Domain.Common;
using Caixinhas.Domain.Repositories;
using Caixinhas.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Caixinhas.Infrastructure.Persistence;

/// <summary>
/// Unit of Work sobre o DbContext do EF Core. Expõe os repositórios,
/// centraliza o SaveChanges, despacha os eventos de domínio e oferece a
/// execução de operações dentro de uma transação de banco.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly CaixinhasDbContext _context;
    private readonly IDomainEventDispatcher _dispatcher;
    private readonly ILogger<UnitOfWork> _logger;

    private IDbContextTransaction? _transacao;
    private bool _disposed;

    private IObjetivoRepository? _objetivos;
    private ITransacaoRepository? _transacoes;

    public UnitOfWork(
        CaixinhasDbContext context,
        IDomainEventDispatcher dispatcher,
        ILogger<UnitOfWork> logger)
    {
        _context = context;
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public IObjetivoRepository Objetivos => _objetivos ??= new ObjetivoRepository(_context);
    public ITransacaoRepository Transacoes => _transacoes ??= new TransacaoRepository(_context);

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        // Coleta os eventos antes de salvar, pois o SaveChanges altera o rastreamento.
        var entidadesComEventos = _context.ChangeTracker
            .Entries<BaseEntity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count != 0)
            .ToList();

        var eventos = entidadesComEventos
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Limpa antes de despachar para evitar publicar o mesmo evento duas vezes.
        foreach (var entidade in entidadesComEventos)
            entidade.LimparEventos();

        var afetados = await _context.SaveChangesAsync(cancellationToken);

        if (eventos.Count != 0)
            await _dispatcher.DespacharAsync(eventos, cancellationToken);

        return afetados;
    }

    /// <summary>
    /// Executa a operação dentro de uma transação de banco. Se a operação ou
    /// o commit falharem, nada é gravado — é isso que garante que a transação
    /// nova e o ValorTotal atualizado da caixinha nunca fiquem divergentes.
    /// Se já existe uma transação aberta, reaproveita (não aninha).
    /// </summary>
    public async Task<T> ExecutarEmTransacaoAsync<T>(
        Func<CancellationToken, Task<T>> operacao,
        CancellationToken cancellationToken = default)
    {
        // Transação já aberta por um chamador acima: participa dela.
        if (_transacao is not null)
            return await operacao(cancellationToken);

        await using var transacao = await _context.Database
            .BeginTransactionAsync(cancellationToken);

        _transacao = transacao;

        try
        {
            var resultado = await operacao(cancellationToken);

            await transacao.CommitAsync(cancellationToken);

            return resultado;
        }
        catch (Exception ex)
        {
            await transacao.RollbackAsync(cancellationToken);

            _logger.LogWarning(
                "Transação desfeita por {Excecao}: {Mensagem}", ex.GetType().Name, ex.Message);

            throw;
        }
        finally
        {
            _transacao = null;
        }
    }

    public async Task IniciarTransacaoAsync(CancellationToken cancellationToken = default)
    {
        if (_transacao is not null) return;
        _transacao = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task ConfirmarTransacaoAsync(CancellationToken cancellationToken = default)
    {
        if (_transacao is null) return;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transacao.CommitAsync(cancellationToken);
        }
        finally
        {
            await _transacao.DisposeAsync();
            _transacao = null;
        }
    }

    public async Task DesfazerTransacaoAsync(CancellationToken cancellationToken = default)
    {
        if (_transacao is null) return;

        try
        {
            await _transacao.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transacao.DisposeAsync();
            _transacao = null;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _transacao?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
