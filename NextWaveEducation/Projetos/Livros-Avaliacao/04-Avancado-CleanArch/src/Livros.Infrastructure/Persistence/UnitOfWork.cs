using Livros.Application.Common.Interfaces;
using Livros.Domain.Common;
using Livros.Domain.Repositories;
using Livros.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Livros.Infrastructure.Persistence;

/// <summary>
/// Unit of Work sobre o DbContext do EF Core. Expõe os repositórios,
/// centraliza o SaveChanges e despacha os eventos de domínio das entidades
/// rastreadas depois de persistir as alterações.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly LivrosDbContext _context;
    private readonly IDomainEventDispatcher _dispatcher;
    private IDbContextTransaction? _transacao;
    private bool _disposed;

    private ILivroRepository? _livros;
    private IUsuarioRepository? _usuarios;
    private IAvaliacaoRepository? _avaliacoes;

    public UnitOfWork(LivrosDbContext context, IDomainEventDispatcher dispatcher)
    {
        _context = context;
        _dispatcher = dispatcher;
    }

    public ILivroRepository Livros => _livros ??= new LivroRepository(_context);
    public IUsuarioRepository Usuarios => _usuarios ??= new UsuarioRepository(_context);
    public IAvaliacaoRepository Avaliacoes => _avaliacoes ??= new AvaliacaoRepository(_context);

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

        // Limpa antes de despachar para evitar que o mesmo evento seja publicado duas vezes.
        foreach (var entidade in entidadesComEventos)
            entidade.LimparEventos();

        var afetados = await _context.SaveChangesAsync(cancellationToken);

        if (eventos.Count != 0)
            await _dispatcher.DespacharAsync(eventos, cancellationToken);

        return afetados;
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
