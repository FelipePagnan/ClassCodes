using System.Reflection;
using BancoSangue.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BancoSangue.Infrastructure.Persistence;

public class BancoSangueDbContext : DbContext
{
    public BancoSangueDbContext(DbContextOptions<BancoSangueDbContext> options) : base(options) { }

    public DbSet<Doador> Doadores => Set<Doador>();
    public DbSet<Doacao> Doacoes => Set<Doacao>();
    public DbSet<Endereco> Enderecos => Set<Endereco>();
    public DbSet<EstoqueSangue> EstoquesSangue => Set<EstoqueSangue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mapeia as entidades a partir das classes IEntityTypeConfiguration<T>
        // deste assembly (um arquivo por entidade).
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
