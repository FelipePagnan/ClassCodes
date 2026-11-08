using System.Reflection;
using Livros.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Livros.Infrastructure.Persistence;

public class LivrosDbContext : DbContext
{
    public LivrosDbContext(DbContextOptions<LivrosDbContext> options) : base(options) { }

    public DbSet<Livro> Livros => Set<Livro>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mapeia as entidades a partir das classes IEntityTypeConfiguration<T>
        // deste assembly (um arquivo por entidade).
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
