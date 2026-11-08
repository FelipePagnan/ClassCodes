using System.Reflection;
using Livros.Intermediario.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Livros.Intermediario.Infraestrutura.Dados;

public class LivrosDbContext : DbContext
{
    public LivrosDbContext(DbContextOptions<LivrosDbContext> options) : base(options) { }

    public DbSet<Livro> Livros => Set<Livro>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as classes IEntityTypeConfiguration<T> deste assembly,
        // mantendo o mapeamento de cada entidade em seu próprio arquivo.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
