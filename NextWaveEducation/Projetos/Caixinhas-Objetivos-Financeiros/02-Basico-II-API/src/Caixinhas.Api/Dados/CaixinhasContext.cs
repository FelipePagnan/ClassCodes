using Caixinhas.Api.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Caixinhas.Api.Dados;

public class CaixinhasContext : DbContext
{
    public CaixinhasContext(DbContextOptions<CaixinhasContext> options) : base(options) { }

    public DbSet<ObjetivoFinanceiro> Objetivos => Set<ObjetivoFinanceiro>();
    public DbSet<Transacao> Transacoes => Set<Transacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ObjetivoFinanceiro>()
            .HasMany(o => o.Transacoes)
            .WithOne(t => t.ObjetivoFinanceiro)
            .HasForeignKey(t => t.ObjetivoFinanceiroId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ObjetivoFinanceiro>()
            .Property(o => o.QuantidadeAlvo)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ObjetivoFinanceiro>()
            .Property(o => o.ValorTotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<ObjetivoFinanceiro>()
            .Property(o => o.QuantidadeIdealAporteMensal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Transacao>()
            .Property(t => t.Quantidade)
            .HasPrecision(18, 2);

        // Soft delete: os registros marcados como deletados somem das
        // consultas automaticamente, sem precisar de Where em cada query.
        modelBuilder.Entity<ObjetivoFinanceiro>()
            .HasQueryFilter(o => !o.EstaDeletado);

        modelBuilder.Entity<Transacao>()
            .HasQueryFilter(t => !t.EstaDeletado);

        // Propriedades calculadas não têm coluna no banco.
        modelBuilder.Entity<ObjetivoFinanceiro>().Ignore(o => o.ValorRestante);
        modelBuilder.Entity<ObjetivoFinanceiro>().Ignore(o => o.PercentualConcluido);
        modelBuilder.Entity<ObjetivoFinanceiro>().Ignore(o => o.MetaAlcancada);
        modelBuilder.Entity<ObjetivoFinanceiro>().Ignore(o => o.DiasRestantes);
        modelBuilder.Entity<ObjetivoFinanceiro>().Ignore(o => o.AceitaTransacoes);
        modelBuilder.Entity<Transacao>().Ignore(t => t.Efeito);

        base.OnModelCreating(modelBuilder);
    }
}
