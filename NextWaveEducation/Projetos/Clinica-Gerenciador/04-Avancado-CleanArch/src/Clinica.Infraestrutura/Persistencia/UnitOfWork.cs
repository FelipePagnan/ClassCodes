using Clinica.Aplicacao.Comum;
using Clinica.Dominio.Entidades;
using Clinica.Dominio.Eventos;
using Clinica.Dominio.Repositorios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Clinica.Infraestrutura.Persistencia;

/// <summary>
/// Unit of Work sobre o DbContext.
///
/// Dois trabalhos, nesta ordem:
///
/// 1. Gravar. Os repositórios só marcam o que mudou; quem grava é daqui, uma
///    vez por caso de uso. É o que permite a um handler mexer em duas
///    entidades e as duas irem no mesmo SaveChanges.
///
/// 2. Publicar os eventos de domínio — depois de a gravação dar certo. A
///    ordem não é detalhe: publicar antes significaria enviar ao paciente a
///    confirmação de um atendimento que a transação ainda pode desfazer.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ClinicaDbContext _context;
    private readonly IDespachanteDeEventos _despachante;
    private readonly ILogger<UnitOfWork> _logger;

    /// <summary>
    /// Transação aberta por este Unit of Work, se houver. Serve para que uma
    /// chamada aninhada a ExecutarEmTransacaoAsync participe da transação em
    /// curso em vez de tentar abrir outra — o que o provider recusaria.
    /// </summary>
    private IDbContextTransaction? _transacao;

    public UnitOfWork(
        ClinicaDbContext context,
        IDespachanteDeEventos despachante,
        ILogger<UnitOfWork> logger)
    {
        _context = context;
        _despachante = despachante;
        _logger = logger;
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        // Os eventos são recolhidos e limpos ANTES de gravar. Se ficassem nas
        // entidades, um segundo CommitAsync no mesmo escopo os publicaria de
        // novo — e o paciente receberia a mesma confirmação duas vezes.
        var eventos = RecolherEventos();

        var afetados = await _context.SaveChangesAsync(cancellationToken);

        if (eventos.Count > 0)
        {
            _logger.LogDebug("Publicando {Quantidade} evento(s) de domínio.", eventos.Count);
            await _despachante.DespacharAsync(eventos, cancellationToken);
        }

        return afetados;
    }

    private List<IEventoDeDominio> RecolherEventos()
    {
        var entidades = _context.ChangeTracker
            .Entries<EntidadeBase>()
            .Select(entrada => entrada.Entity)
            .Where(entidade => entidade.Eventos.Count > 0)
            .ToList();

        var eventos = entidades.SelectMany(e => e.Eventos).ToList();

        foreach (var entidade in entidades)
            entidade.LimparEventos();

        return eventos;
    }

    public async Task<T> ExecutarEmTransacaoAsync<T>(
        Func<CancellationToken, Task<T>> operacao,
        CancellationToken cancellationToken = default)
    {
        // Já existe transação neste escopo: a operação participa dela. Quem
        // abriu é quem confirma ou desfaz.
        if (_transacao is not null || _context.Database.CurrentTransaction is not null)
            return await operacao(cancellationToken);

        _transacao = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var resultado = await operacao(cancellationToken);

            await _transacao.CommitAsync(cancellationToken);

            return resultado;
        }
        catch
        {
            await _transacao.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transacao.DisposeAsync();
            _transacao = null;
        }
    }
}
