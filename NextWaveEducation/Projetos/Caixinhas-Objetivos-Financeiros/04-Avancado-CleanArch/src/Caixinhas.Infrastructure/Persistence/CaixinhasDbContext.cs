using System.Reflection;
using Caixinhas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Caixinhas.Infrastructure.Persistence;

public class CaixinhasDbContext : DbContext
{
    public CaixinhasDbContext(DbContextOptions<CaixinhasDbContext> options) : base(options) { }

    public DbSet<ObjetivoFinanceiro> Objetivos => Set<ObjetivoFinanceiro>();
    public DbSet<Transacao> Transacoes => Set<Transacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mapeia as entidades a partir das classes IEntityTypeConfiguration<T>
        // deste assembly (um arquivo por entidade).
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
