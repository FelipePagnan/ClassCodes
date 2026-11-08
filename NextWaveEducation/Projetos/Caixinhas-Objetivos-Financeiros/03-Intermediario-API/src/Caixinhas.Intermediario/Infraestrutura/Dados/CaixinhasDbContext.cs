using System.Reflection;
using Caixinhas.Intermediario.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Caixinhas.Intermediario.Infraestrutura.Dados;

public class CaixinhasDbContext : DbContext
{
    public CaixinhasDbContext(DbContextOptions<CaixinhasDbContext> options) : base(options) { }

    public DbSet<ObjetivoFinanceiro> Objetivos => Set<ObjetivoFinanceiro>();
    public DbSet<Transacao> Transacoes => Set<Transacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as classes IEntityTypeConfiguration<T> deste assembly,
        // mantendo o mapeamento de cada entidade em seu próprio arquivo.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
